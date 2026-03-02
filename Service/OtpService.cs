namespace Service
{
    public class OtpService : IOtpService
    {
        private readonly Dictionary<string, (string otp, DateTime expiry)> _otpStore = new();
        private readonly object _lockObject = new();

        public string GenerateOtp(int length = 6)
        {
            const string digits = "0123456789";
            var random = new Random();
            var otp = new string(Enumerable.Range(0, length)
                .Select(_ => digits[random.Next(digits.Length)])
                .ToArray());
            return otp;
        }

        public bool SaveOtp(string email, string otp, int expiryMinutes = 10)
        {
            lock (_lockObject)
            {
                var expiry = DateTime.UtcNow.AddMinutes(expiryMinutes);
                _otpStore[email] = (otp, expiry);
                return true;
            }
        }

        public bool VerifyOtp(string email, string otp)
        {
            lock (_lockObject)
            {
                if (!_otpStore.ContainsKey(email))
                    return false;

                var (storedOtp, expiry) = _otpStore[email];

                if (DateTime.UtcNow > expiry)
                {
                    _otpStore.Remove(email);
                    return false;
                }

                return storedOtp == otp;
            }
        }

        public void DeleteOtp(string email)
        {
            lock (_lockObject)
            {
                _otpStore.Remove(email);
            }
        }
    }
}