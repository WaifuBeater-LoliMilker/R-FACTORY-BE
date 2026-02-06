using Dapper;
using Microsoft.Extensions.Hosting;
using R_Factory_BE.Middlewares;
using R_Factory_BE.Models;
using System.Data;

namespace R_Factory_BE.Services
{
    public class LogService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IGenericRepo _genericRepo;

        public LogService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RunAggregation();
                } catch (Exception ex)
                {
                    Console.WriteLine($"Error in LogService: {ex.Message}");
                }
                await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
            }
        }

        private async Task RunAggregation()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            var now = DateTime.Now;
            var from = now.AddMinutes(-30);
            var to = now;

            try
            {
                var splitMinutes = await db.ExecuteAsync(
                    "CALL spAggregateEnergyMinutely(@from, @to)",
                    new { from, to }
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
            }

            try
            {
                var splitHours = await db.ExecuteAsync(
                    "CALL spAggregateEnergyHourly(@from, @to)",
                    new { from, to }
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
            }

            try
            {
                var splitDays = await db.ExecuteAsync(
                    "CALL spAggregateEnergyDaily(@from, @to)",
                    new { from, to }
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
            }

            try
            {
                var splitMonths = await db.ExecuteAsync(
                    "CALL spAggregateEnergyMonthly(@year, @month)",
                    new { year = to.Year, month = to.Month }
                );
            }
            catch (Exception ex)
            {
                ErrorLogger.Write(ex);
            }


        }
    }
}
