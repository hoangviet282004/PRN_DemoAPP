using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Model.Models;
using Service;
using Service.Dtos.Request;
using Service.Dtos.Response;
using Swashbuckle.AspNetCore.Annotations;

namespace MyDemoAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountAPI : ControllerBase
    {
        private readonly IBearAccountService _service;

        public AccountAPI(IBearAccountService service)
        {
            _service = service;
        }

        [HttpGet("{email}")]
        public IActionResult GetAccountByEmail(string email)
        {
            var account = _service.GetAccount(email);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }

        [HttpGet("GetAccountById/{accountId}")]
        public async Task<IActionResult> GetAccountById(int accountId)
        {
            var account = await _service.GetBearAccount(accountId);
            if (account == null)
            {
                return NotFound();
            }
            return Ok(account);
        }


        [HttpPut("update-profile/{id}")]
        [Authorize(Roles = ("1,2"))] // Chỉ Admin (1) và User (2) mới được cập nhật hồ sơ
        public async Task<IActionResult> UpdateProfile(int id, [FromBody] UpdateAcccountRequest request)
        {
            // Kiểm tra lỗi cú pháp JSON (nếu dấu ngoặc sai ModelState sẽ Invalid)
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ErrorResponse(400, "Dữ liệu JSON không hợp lệ.", errors));
            }

            var result = await _service.UpdateAccountByIdAsync(id, request);

            if (result == null)
            {
                return NotFound(new ErrorResponse(404, $"Không tìm thấy tài khoản ID: {id}"));
            }

            return Ok(new ApiResponse<BearAccount>(200, "Cập nhật thành công!", result));
        }

        [HttpGet("all-accounts")]
        public async Task<IActionResult> GetAllAccounts()
        {
            var accounts = await _service.GetAllAccountsAsync();
            return Ok(new ApiResponse<List<BearAccount>>(200, "Lấy tất cả tài khoản thành công.", accounts));
        }

        [HttpDelete("delete-account/{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            var result = await _service.DeleteAccountByIdAsync(id);
            if (result == null)
            {
                return NotFound(new ErrorResponse(404, $"Không tìm thấy tài khoản ID: {id}"));
            }
            return Ok(new ApiResponse<string>(200, "Xóa tài khoản thành công.", null));
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchAccounts([FromQuery]  string? userName, [FromQuery] string? email)
        {
            var results = await _service.SearchBearByNameAsync(userName ?? string.Empty, email ?? string.Empty);
            return Ok(new ApiResponse<List<BearAccount>>(200, "Tìm kiếm tài khoản thành công.", results));
        }

        /// <summary>
        /// Search bear accounts by enum type and search term.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="searchTerm"></param>
        /// <returns></returns>

        [HttpGet("searchByEnums")]
        public async Task<IActionResult> Search([FromQuery] SearchRequest type, [FromQuery] string? searchTerm)
        {
            // Controller chỉ nhận dữ liệu và đẩy xuống Service
            // Dùng ?? "" để xử lý trường hợp searchTerm bị null
            var result = await _service.SearchBearAsync(type, searchTerm ?? "");

            if (result == null || !result.Any())
            {
                return NotFound(new ErrorResponse(404, $"Không tìm thấy tài khoản {searchTerm}"));
            }

            return Ok(new ApiResponse<List<BearAccount>>(200, "Tìm kiếm tài khoản thành công.", result));
        }
    }
}
