using Model.Models;
using Service.Dtos.Request;
using Service.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public interface IBearAccountService
    {
        BearAccount GetAccount(string email);

        Task<BearAccount> GetBearAccount(int accountId);
        
        BearAccount Login(string email, string password);

        Task<ApiResponse<string>> ForgotPasswordAsync(string email);
        Task<ApiResponse<string>> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword);

        Task<BearAccount> UpdateAccountByIdAsync(int accountId, UpdateAcccountRequest request);

        Task<List<BearAccount>> GetAllAccountsAsync();

        Task<BearAccount> DeleteAccountByIdAsync(int accountId);

        Task<List<BearAccount>> SearchBearByNameAsync(string userName, string email);

        Task<List<BearAccount>> SearchBearAsync(SearchRequest type, string searchTerm);
    }
}
