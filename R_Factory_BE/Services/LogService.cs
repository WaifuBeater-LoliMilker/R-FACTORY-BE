using Dapper;
using Microsoft.Extensions.Hosting;
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
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task RunAggregation()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDbConnection>();

            var now = DateTime.Now;
            var from = now.AddHours(-1);
            //var from = now.AddMonths(-1);
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
                Console.WriteLine("Minutely error: " + ex.Message);
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
                Console.WriteLine("Hourly error: " + ex.Message);
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
                Console.WriteLine("Daily error: " + ex.Message);
            }
            ;

            try
            {
                var splitMonths = await db.ExecuteAsync(
                    "CALL spAggregateEnergyMonthly(@year, @month)",
                    new { year = to.Year, month = to.Month }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Monthly error: " + ex.Message);
            }

   
        }
    }
}
