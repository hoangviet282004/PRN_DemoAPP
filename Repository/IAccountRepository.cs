using Model.Models;
using System.Linq.Expressions;

namespace Repository
{
    public interface IAccountRepository
    {
        BearAccount LoginBearAccount(string email, string password);
        Task<BearAccount> GetBearAccountAsyncById(int accountId);
        BearAccount GetAccountByUserName(string email);
        Task<bool> UpdateBearAccountAsync(BearAccount account);

        Task<BearAccount> UpdateAccountByIdAsync(int accountId, BearAccount updatedAccount);

        Task<BearAccount> DeleteAccountByIdAsync(int accountId);

        Task<List<BearAccount>> GetAllAccountsAsync();

        Task<List<BearAccount>> SearchBearByNameAsync(string userName, string email);

        Task<List<BearAccount>> SearchBearByFilterAsync(Expression<Func<BearAccount, bool>> filter);
    }
}
