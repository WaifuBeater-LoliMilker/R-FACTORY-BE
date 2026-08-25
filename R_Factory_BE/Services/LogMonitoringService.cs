using System.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using R_Factory_BE.Middlewares;

namespace R_Factory_BE.Services;

/// <summary>
/// Settings bound from the "LogMonitoring" appsettings section.
/// </summary>
public class LogMonitoringOptions
{
    /// <summary>Master on/off switch for the monitoring service.</summary>
    public bool Enabled { get; set; } = true;

    /// <summary>How often (in minutes) the service checks for fresh logs.</summary>
    public int CheckIntervalMinutes { get; set; } = 5;

    /// <summary>
    /// If no log has been received for longer than this (in minutes) an outage is declared
    /// and a warning email is sent.
    /// </summary>
    public int WarningThresholdMinutes { get; set; } = 30;

    /// <summary>
    /// While an outage persists, a reminder email is re-sent at most every this many minutes.
    /// </summary>
    public int ReminderIntervalMinutes { get; set; } = 360;

    /// <summary>Subject line used for the warning email.</summary>
    public string Subject { get; set; } = "[R_Factory] Ngừng nhận log từ nhà máy";
}

/// <summary>
/// Background service that monitors <c>device_parameter_logs</c> and sends a warning
/// email whenever the factory stops sending fresh log data for an extended period.
/// </summary>
public class LogMonitoringService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IEmailService _emailService;
    private readonly LogMonitoringOptions _options;
    private readonly ILogger<LogMonitoringService> _logger;

    private readonly object _lock = new();
    private DateTimeOffset _lastAlertAt = DateTimeOffset.MinValue;
    private bool _wasHealthy = true;

    public LogMonitoringService(
        IServiceScopeFactory scopeFactory,
        IEmailService emailService,
        IOptions<LogMonitoringOptions> options,
        ILogger<LogMonitoringService> logger)
    {
        _scopeFactory = scopeFactory;
        _emailService = emailService;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("LogMonitoringService disabled by configuration.");
            return;
        }

        _logger.LogInformation(
            "LogMonitoringService started (threshold: {WarningThresholdMinutes} min, check every {CheckIntervalMinutes} min).",
            _options.WarningThresholdMinutes, _options.CheckIntervalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckForStaleLogsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
                _logger.LogError(ex, "Error while checking log freshness.");
            }

            await Task.Delay(TimeSpan.FromMinutes(_options.CheckIntervalMinutes), stoppingToken);
        }
    }

    private async Task CheckForStaleLogsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

        var maxLogTime = await db.QuerySingleOrDefaultAsync<DateTime?>(
            "SELECT MAX(LogTime) FROM device_parameter_logs",
            cancellationToken);

        // No data at all yet — treat as healthy to avoid flooding inboxes on a brand-new install.
        if (maxLogTime is null)
        {
            lock (_lock)
            {
                _wasHealthy = true;
            }
            return;
        }

        var sinceLastLog = DateTime.Now - maxLogTime.Value;
        var isHealthy = sinceLastLog <= TimeSpan.FromMinutes(_options.WarningThresholdMinutes);

        bool shouldSend;
        lock (_lock)
        {
            if (isHealthy)
            {
                // Fresh data is flowing again; reset so the next outage triggers a fresh alert.
                _wasHealthy = true;
                return;
            }

            // Declared an outage.
            shouldSend = _wasHealthy
                         || (DateTime.Now - _lastAlertAt) >= TimeSpan.FromMinutes(_options.ReminderIntervalMinutes);

            _wasHealthy = false;
            if (shouldSend)
            {
                _lastAlertAt = DateTime.Now;
            }
        }

        if (!shouldSend)
        {
            return;
        }

        await SendAlertAsync(maxLogTime.Value, sinceLastLog, cancellationToken);
    }

    private async Task SendAlertAsync(DateTime lastLogTime, TimeSpan downtime, CancellationToken cancellationToken)
    {
        var body =
            $"Không nhận được log mới từ nhà máy trong một khoảng thời gian dài.\n\n" +
            $"Thời gian nhận được log gần nhất : {lastLogTime:yyyy-MM-dd HH:mm:ss}\n" +
            $"Thời gian đã ngừng nhận log    : {downtime.TotalMinutes:0} phút\n" +
            $"Thời điểm kiểm tra             : {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n\n" +
            "Vui lòng kiểm tra kết nối mạng, máy tính gửi log (Winforms) và thiết bị đo tại nhà máy.";

        try
        {
            await _emailService.SendWarningAsync(_options.Subject, body, cancellationToken);
        }
        catch (Exception ex)
        {
            // Do not let a mail failure crash the monitoring loop.
            ErrorLogger.Write(ex);
            _logger.LogError(ex, "Failed to send log-monitoring warning email.");
        }
    }
}
