using Model.Models;
using Repository;
using Service.Dtos.Request;
using Service.Dtos.Response;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Text;

namespace Service
{
    public class BearAccountService : IBearAccountService
    {
        private readonly IAccountRepository _repository;
        private readonly IOtpService _otpService;
        private readonly IEmailService _emailService;

        public BearAccountService(IAccountRepository accountRepository, IOtpService otpService, IEmailService emailService)
        {
            _repository = accountRepository;
            _otpService = otpService;
            _emailService = emailService;
        }

        public Task<BearAccount> GetBearAccount(int accountId)
        {
            var account = _repository.GetBearAccountAsyncById(accountId);
            return account;
        }

        public BearAccount Login(string email, string password)
        {
            return _repository.LoginBearAccount(email, password);
        }

        public BearAccount GetAccount(string email)
        {
            var account = _repository.GetAccountByUserName(email);
            return account;
        }

        public async Task<ApiResponse<string>> ForgotPasswordAsync(string email)
        {
            try
            {
                var account = _repository.GetAccountByUserName(email);
                if (account == null)
                    return new ApiResponse<string>(404, "Email không tồn tại.");

                // Generate OTP
                var otp = _otpService.GenerateOtp();
                _otpService.SaveOtp(email, otp);

                // Send OTP via email
                var emailSent = await _emailService.SendOtpEmailAsync(email, otp);
                if (!emailSent)
                    return new ApiResponse<string>(500, "Lỗi gửi email.");

                return new ApiResponse<string>(200, "Mã OTP đã được gửi đến email của bạn.");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(500, $"Lỗi: {ex.Message}");
            }
        }

        public async Task<ApiResponse<string>> VerifyOtpAndResetPasswordAsync(string email, string otp, string newPassword)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(otp) || string.IsNullOrWhiteSpace(newPassword))
                    return new ApiResponse<string>(400, "Email, OTP và mật khẩu mới không được để trống.");

                // Verify OTP
                if (!_otpService.VerifyOtp(email, otp))
                    return new ApiResponse<string>(400, "Mã OTP không hợp lệ hoặc đã hết hạn.");

                var account = _repository.GetAccountByUserName(email);
                if (account == null)
                    return new ApiResponse<string>(404, "Email không tồn tại.");

                // Update password
                account.Password = HashPassword(newPassword);
                //account.UpdatedDate = DateTime.UtcNow;

                // Assuming repository has UpdateBearAccountAsync method
                var result = await _repository.UpdateBearAccountAsync(account);
                _otpService.DeleteOtp(email);

                if (result)
                    return new ApiResponse<string>(200, "Đặt lại mật khẩu thành công.");

                return new ApiResponse<string>(500, "Lỗi cập nhật mật khẩu.");
            }
            catch (Exception ex)
            {
                return new ApiResponse<string>(500, $"Lỗi: {ex.Message}");
            }
        }

        // Helper method to hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        public async Task<BearAccount> UpdateAccountByIdAsync(int accountId, UpdateAcccountRequest request)
        {
            // LOGIC: Bạn có thể xử lý dữ liệu ở đây
            // Ví dụ: Trim khoảng trắng, chuyển Email về chữ thường
            var newAccount = new BearAccount
            {
                UserName = request.UserName?.Trim(),
                FullName = request.FullName?.Trim(),
                Email = request.Email?.Trim().ToLower(),
                Phone = request.Phone?.Trim(),
            };

            // Gọi xuống Repository và truyền toàn bộ object đã xử lý logic
            // Repository sẽ dùng Anonymous Object để tránh lỗi ID như tôi đã hướng dẫn
            var updatedAccount = await _repository.UpdateAccountByIdAsync(accountId, newAccount);

            return updatedAccount;
        }

        public Task<List<BearAccount>> GetAllAccountsAsync()
        {
            var accounts = _repository.GetAllAccountsAsync();
            return accounts;
        }

        public Task<BearAccount> DeleteAccountByIdAsync(int accountId)
        {
            var deletedAccount = _repository.DeleteAccountByIdAsync(accountId);
            return deletedAccount;
        }

        public Task<List<BearAccount>> SearchBearByNameAsync(string userName, string email)
        {
            var accounts = _repository.SearchBearByNameAsync(userName, email);
            return accounts;
        }

        public async Task<List<BearAccount>> SearchBearAsync(SearchRequest type, string searchTerm)
        {
            // Tạo công thức lọc (Predicate) dựa trên Enum
            Expression<Func<BearAccount, bool>> filter = type switch
            {
                SearchRequest.userName => a => a.UserName.Contains(searchTerm),
                SearchRequest.email => a => a.Email.Contains(searchTerm),
                _ => a => true // Trả về tất cả nếu không khớp
            };

            // Gửi công thức này xuống cho Repository thực hiện
            return await _repository.SearchBearByFilterAsync(filter);

        }
    }
}
