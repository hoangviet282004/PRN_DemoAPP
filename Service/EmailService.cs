using System.Net;
using System.Net.Mail;

namespace Service
{
    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderPassword;

        public EmailService(string smtpServer, int smtpPort, string senderEmail, string senderPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _senderEmail = senderEmail;
            _senderPassword = senderPassword;
        }

        public async Task<bool> SendOtpEmailAsync(string email, string otp)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(_senderEmail, _senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_senderEmail),
                        Subject = "Mã OTP xác nhận đổi mật khẩu",
                        Body = $@"
                            <html>
                            <body>
                                <h2>Mã OTP của bạn</h2>
                                <p>Mã OTP của bạn là: <strong>{otp}</strong></p>
                                <p>Mã này sẽ hết hạn trong 10 phút.</p>
                                <p>Nếu bạn không yêu cầu mã này, vui lòng bỏ qua email này.</p>
                            </body>
                            </html>
                        ",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await client.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi gửi email: {ex.Message}");
                return false;
            }
        }
    }
}