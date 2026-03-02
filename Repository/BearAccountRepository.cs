using Model.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository
{
    public class BearAccountRepository : IAccountRepository
    {
        private readonly BearManagementContext _context;

        public BearAccountRepository(BearManagementContext context)
        {
            _context = context;
        }

        public BearAccount LoginBearAccount(string email, string password)
        {
            return _context.BearAccounts.FirstOrDefault(x => x.Email == email && x.Password == password);
        }

        public async Task<BearAccount> GetBearAccountAsyncById(int accountId)
        {
            return await _context.BearAccounts.FirstOrDefaultAsync(x => x.AccountId == accountId);
        }

        public BearAccount GetAccountByUserName(string email)
        {
            return _context.BearAccounts.FirstOrDefault(x => x.Email == email);
        }

        public async Task<bool> UpdateBearAccountAsync(BearAccount account)
        {
            try
            {
                _context.BearAccounts.Update(account);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public  async Task<BearAccount> UpdateAccountByIdAsync(int accountId, BearAccount updatedAccount)
        {
            // 1. Tìm bản ghi gốc trong DB
            var existingAccount = await _context.BearAccounts.FindAsync(accountId);
            if (existingAccount == null) return null;

            // 2. Chỉ cập nhật những trường thực sự có giá trị (tránh đè null vào Password)
            // Chúng ta chỉ liệt kê những trường có trong UpdateAcccountRequest
            _context.Entry(existingAccount).CurrentValues.SetValues(new
            {
                // Nếu trường nào trong updatedAccount bị null (do request ko gửi), 
                // thì ta lấy lại giá trị cũ của existingAccount để giữ nguyên
                UserName = updatedAccount.UserName ?? existingAccount.UserName,
                FullName = updatedAccount.FullName ?? existingAccount.FullName,
                Email = updatedAccount.Email ?? existingAccount.Email,
                Phone = updatedAccount.Phone ?? existingAccount.Phone
            });

            // 3. Lưu xuống SQL Server
            await _context.SaveChangesAsync();
            return existingAccount;
        }

        public async Task<BearAccount> DeleteAccountByIdAsync(int accountId)
        {
           var account = await  _context.BearAccounts.FirstOrDefaultAsync(a => a.AccountId == accountId);
            _context.BearAccounts.Remove(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<List<BearAccount>> GetAllAccountsAsync()
        {
            var accounts = await _context.BearAccounts.ToListAsync();
            return accounts;
        }

        public async Task<List<BearAccount>> SearchBearByNameAsync(string userName, string email)
        {
            // Khởi tạo query ban đầu
            var query = _context.BearAccounts.AsQueryable();

            // Kiểm tra nếu có userName thì mới lọc
            if (!string.IsNullOrEmpty(userName))
            {
                query = query.Where(a => a.UserName.Contains(userName));
            }

            // Kiểm tra nếu có email thì mới lọc
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(a => a.Email.Contains(email));
            }

            return await query.ToListAsync();
        }

        public async Task<List<BearAccount>> SearchBearByFilterAsync(Expression<Func<BearAccount, bool>> filter)
        {
            return await _context.BearAccounts.Where(filter).ToListAsync();
        }
    }
}
