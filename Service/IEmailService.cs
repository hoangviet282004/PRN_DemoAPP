namespace Service
{
    public interface IEmailService
    {
        Task<bool> SendOtpEmailAsync(string email, string otp);
    }
}