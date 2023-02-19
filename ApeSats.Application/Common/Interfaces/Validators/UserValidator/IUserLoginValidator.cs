using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces.Validators
{
    public interface IUserLoginValidator
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
