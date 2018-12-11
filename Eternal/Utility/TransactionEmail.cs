using Eternal.Models;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using System.Threading.Tasks;

namespace Eternal.Utility
{
    public class TransactionEmail 
    {
        /*private readonly IEmailService _emailService;

        public TransactionEmail(IEmailService emailService)
            => _emailService = emailService;
        

        public void SendRegistrationEmail(User user)
        {
            EmailMessage message = new EmailMessage();
            message.ToAddresses.Add(new EmailAddress { Username = user.Username, Address = user.Email });
            message.Content = 
                $"<p style='font-size:16px;text-align:center;'>Welcome, {user.Username}!</p><br /><a style='background-color:blue;color:white;font-size:15px;padding:15px;text-align:center;text-decoration:none;display:inline-block;' href='http://Localhost:49288/Users/Activate?userId={user.UserID}&token={user.Token}'>Activate My Account</a>";
            message.Subject = "EternalMeta Registration";
            message.FromAddresses.Add(new EmailAddress { Username = "EternalMeta", Address = "noreply@eternalmeta" });
            message.Send(message);
        }

        public void SendRecoveryEmail(User user)
        {
            EmailMessage message = new EmailMessage();
            message.ToAddresses.Add(new EmailAddress { Username = user.Username, Address = user.Email });
            message.Content =
                $"<p style='font-size:16px;text-align:center;'>Oops!<br />It looks like you forgot your password</p><br /><a style='background-color:blue;color:white;font-size:15px;padding:15px;text-align:center;text-decoration:none;display:inline-block;' href='http://Localhost:49288/Users/ResetPassword?userId={user.UserID}&token={user.Token}'>Reset Password</a>";
        }*/

        public class EmailMessage
        {
            public User User { get; set; }
            public string Subject { get; set; }
            public string Content { get; set; }
        }


        public static async Task SendRegistrationEmail(User user)
        {
            EmailMessage message = new EmailMessage
            {
                User = user,
                Subject = "EternalMeta - Registration",
                Content = $"<center><p style='font-size:16px;text-align:center;'>Welcome, {user.Username}!</p><br /><a style='background-color:blue;color:white;font-size:15px;padding:15px;text-align:center;text-decoration:none;display:inline-block;' href='http://Localhost:49288/Users/Activate?userId={user.UserID}&token={user.Token}'>Activate My Account</a></center>"
            };

            await Send(message);
        }

        public static async Task SendPasswordResetEmail(User user)
        {
            EmailMessage message = new EmailMessage
            {
                User = user,
                Subject = "EternalMeta - Reset Password",
                Content = $"<center><p style='margin:auto;font-size:16px;text-align:center;'>Oops!<br />It looks like you forgot your password</p><br /><a style='background-color:blue;color:white;font-size:15px;padding:15px;text-align:center;text-decoration:none;display:inline-block;' href='http://Localhost:49288/Users/ResetPassword?userId={user.UserID}&token={user.Token}'>Reset Password</a></center>"
            };

            await Send(message);
        }

        public static async Task Send(EmailMessage emailMessage)
        {
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("EternalMeta", "info@eternalmeta.com"));
            message.To.Add(new MailboxAddress(emailMessage.User.Username, emailMessage.User.Email));
            message.Subject = emailMessage.Subject;
            message.Body = new TextPart(TextFormat.Html)
            {
                Text = emailMessage.Content
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                await client.ConnectAsync("smtp.1and1.com", 465, true);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                await client.AuthenticateAsync("info@eternalmeta.com", "password");

                await client.SendAsync(message);

                await client.DisconnectAsync(true);
            }
        }
    }
}
