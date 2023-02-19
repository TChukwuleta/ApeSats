using ApeSats.Core.Entities;
using ApeSats.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApeSats.Application.Common.Interfaces
{
    public interface IAuthService
    {
        Task<(Result result, Core.Entities.User user)> CreateUserAsync(Core.Entities.User user);
        Task<Result> UpdateUserAsync(Core.Entities.User user);
        Task<Result> ChangePasswordAsync(string email, string oldPassword, string newPassword);
        Task<Result> ResetPassword(string email, string password);
        Task<Result> EmailVerification(string email, string otp);
        Task<Result> Login(string email, string password);
        Task<Result> GenerateOTP(string email);
        Task<Result> ValidationOTP(string email, string otp);
        Task<(Result result, Core.Entities.User user)> GetUserByEmail(string email);
        Task<(Result result, Core.Entities.User user)> GetUserById(string userid);
        Task<(Result result, List<Core.Entities.User> users)> GetAllUsers(int skip, int take);
    }
}
