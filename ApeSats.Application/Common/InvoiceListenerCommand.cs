using ApeSats.Application.Common.Interfaces;
using Coravel.Invocable;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common
{
    public class InvoiceListenerCommand : IInvocable
    {
        private readonly IConfiguration _config;
        private readonly IAppDbContext _context;
        private readonly IAuthService _authService;
        public InvoiceListenerCommand(IConfiguration config, IAppDbContext context, IAuthService authService)
        {
            _config = config;
            _context = context;
            _authService = authService;
        }

        public Task Invoke()
        {
            Console.WriteLine("Hey you");
            throw new NotImplementedException();
        }
    }
}
