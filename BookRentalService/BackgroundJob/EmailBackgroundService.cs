using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using BookRentalService.Repository;
using BookRentalService.Services;
namespace BookRentalService.BackgroundJob
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider; // Use IServiceProvider to resolve services
        private readonly ILogger<EmailBackgroundService> _logger;

        public EmailBackgroundService(IServiceProvider serviceProvider, ILogger<EmailBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Background email job is running.");

                try
                {
                    // Create a scope to resolve the IEmailService
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                        await emailService.SendOverdueNotificationsAsync();
                    }

                    _logger.LogInformation("Overdue notifications processed successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while sending overdue notifications.");
                }

                // Wait for an hour before the next execution
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }
    }

}
