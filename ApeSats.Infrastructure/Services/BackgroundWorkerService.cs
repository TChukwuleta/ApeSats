using ApeSats.Application.Arts.Commands;
using ApeSats.Application.Common.Interfaces;
using ApeSats.Application.Lightning.Commands;
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
                    var _authService = scope.ServiceProvider.GetService<IAuthService>();


                    Console.WriteLine("About to start running round of background service");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);


                    // Settle pending art requests
                    Console.WriteLine("About to start settling pending arts requests");
                    var settleArtRequest = new SettleArtBidByAccountCommand();
                    var settleArtRequestHandler = new SettleArtBidByAccountCommandHandler(_context, _authService);
                    await settleArtRequestHandler.Handle(settleArtRequest, stoppingToken);

                    
                    // Record all art requests
                    Console.WriteLine("About to start running recording arts requests");
                    var recordArtRequest = new RecordArtRequestCommand();
                    var recordArtRequestHandler = new RecordArtRequestCommandHandler(_context);
                    await recordArtRequestHandler.Handle(recordArtRequest, stoppingToken);

                    
                    // Listen for lightning settlement
                    Console.WriteLine("About to start running round for lightning settlement automation");
                    var lightningRequest = new ListenForPaymentCommand();
                    var lightningTransactionInitiationHandler = new ListenForPaymentCommandHandler(_lightningService, _context, _authService);
                    lightningTransactionInitiationHandler.Handle(lightningRequest, stoppingToken);

                    Console.WriteLine("Done running round of background service");
                    
                }

            }
        }
    }
}
