namespace Service
{
    public interface IOtpService
    {
        string GenerateOtp(int length = 6);
        bool SaveOtp(string email, string otp, int expiryMinutes = 10);
        bool VerifyOtp(string email, string otp);
        void DeleteOtp(string email);
    }
}