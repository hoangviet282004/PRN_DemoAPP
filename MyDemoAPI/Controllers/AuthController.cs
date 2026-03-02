using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Service;
using Service.Dtos.Request;
using Service.Dtos.Response;
using System.Security.Claims;

namespace MyDemoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IJwtService _jwtService;
        private readonly IBearAccountService _service;

        public AuthController(IJwtService jwtService, IBearAccountService bearAccountService)
        {
            _jwtService = jwtService;
            _service = bearAccountService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(request.Email))
                    errors.Add("Email không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Password))
                    errors.Add("Mật khẩu không được để trống.");

                return BadRequest(new ErrorResponse(400, "Dữ liệu không hợp lệ.", null, errors));
            }

            // 1. Kiểm tra tài khoản
            var bearAccount = _service.Login(request.Email, request.Password);

            if (bearAccount == null)
                return Unauthorized(new ErrorResponse(401, "Email hoặc mật khẩu không chính xác."));

            try
            {
                // 2. Tạo Token (Truyền ID, Email và Role)
                var token = _jwtService.GenerateToken(
                    bearAccount.AccountId.ToString(),
                    bearAccount.Email,
                    bearAccount.RoleId.ToString()!);

                var refreshToken = _jwtService.GenerateRefreshToken();

                var loginResponse = new LoginResponse
                {
                    Token = token,
                    RefreshToken = refreshToken,
                    Expiration = DateTime.UtcNow.AddMinutes(60)
                };

                // 3. Trả về kết quả thành công
                return Ok(new ApiResponse<LoginResponse>(200, "Đăng nhập thành công.", loginResponse));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi tạo token.", null, new List<string> { ex.Message }));
            }
        }

        [HttpGet("profile")]
        [Authorize] // Yêu cầu xác thực người dùng token
        public async Task<IActionResult> GetProfile()
        {
            // Lấy ID người dùng từ Claims trong Token đã gửi lên
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("id")?.Value;

            if (string.IsNullOrWhiteSpace(userIdStr))
                return BadRequest(new ErrorResponse(400, "Token không hợp lệ."));

            if (!int.TryParse(userIdStr, out int accountId))
                return BadRequest(new ErrorResponse(400, "ID người dùng không hợp lệ."));

            try
            {
                var account = await _service.GetBearAccount(accountId);

                if (account == null)
                    return NotFound(new ErrorResponse(404, "Không tìm thấy tài khoản."));

                // Trả về thông tin (ẩn mật khẩu)
                account.Password = null;
                return Ok(new ApiResponse<BearAccount>(200, "Lấy hồ sơ thành công.", account));
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ErrorResponse(500, "Lỗi khi lấy hồ sơ.", null, new List<string> { ex.Message }));
            }
        }

        // ============ Forgot Password ============
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return BadRequest(new ErrorResponse(400, "Email không được để trống."));

            var result = await _service.ForgotPasswordAsync(request.Email);
            return StatusCode(result.StatusCode, result);
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Otp) || string.IsNullOrWhiteSpace(request.NewPassword))
            {
                var errors = new List<string>();
                if (string.IsNullOrWhiteSpace(request.Email))
                    errors.Add("Email không được để trống.");
                if (string.IsNullOrWhiteSpace(request.Otp))
                    errors.Add("OTP không được để trống.");
                if (string.IsNullOrWhiteSpace(request.NewPassword))
                    errors.Add("Mật khẩu mới không được để trống.");

                return BadRequest(new ErrorResponse(400, "Dữ liệu không hợp lệ.", null, errors));
            }

            var result = await _service.VerifyOtpAndResetPasswordAsync(request.Email, request.Otp, request.NewPassword);
            return StatusCode(result.StatusCode, result);
        }
    }
}
