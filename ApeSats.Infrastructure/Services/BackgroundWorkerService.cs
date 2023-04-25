using ApeSats.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ApeSats.Infrastructure.Services
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public BackgroundWorkerService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using (var scope = _serviceProvider.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetService<IAppDbContext>();
                    var _configuration = scope.ServiceProvider.GetService<IConfiguration>();
                    var _lightningService = scope.ServiceProvider.GetService<ILightningService>();

                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

                    Console.WriteLine("About to start running round of background service");


                    /*await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                    Console.WriteLine("About to start running round for bitcoin settlement automation");
                    var bitcoinRequest = new ListenForBitcoinSwapPaymentCommand();
                    var bitcoinTransactionInitiationReversal = new ListenForBitcoinSwapPaymentCommandHandler(_lightningService, _context);*/
                    //await bitcoinTransactionInitiationReversal.Handle(bitcoinRequest, stoppingToken);

                    Console.WriteLine("Done running round of background service");
                }

            }
        }
    }
}
