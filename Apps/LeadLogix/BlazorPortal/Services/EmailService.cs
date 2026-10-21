using BlazorPortal.Data;
using BlazorPortal.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;


namespace WebPortal.Services
{
    public class EmailService : IEmailSender<ApplicationUser>
    {
        private EmailSettingModel? emailSetting = new EmailSettingModel();

        public EmailService(IConfiguration configuration)
        {
            configuration.GetSection("EmailSetting").Bind(emailSetting);
        }







        public async Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
        {
            emailSetting ??= new EmailSettingModel();

            var client = new SmtpClient();
            client.Connect(emailSetting.Host, emailSetting.Port, false);
            client.Authenticate(emailSetting.UserName, emailSetting.Password);


            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(emailSetting.EmailFromName, emailSetting.EmailAddress));
            message.To.Add(new MailboxAddress(user.UserName, email));
            message.Subject = "Confirm your email";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"Confirm your email, Please confirm your account by <a href='{confirmationLink}'>clicking here</a>."
            };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }








        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
        {
            throw new NotImplementedException();
        }









        public async Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
        {
            emailSetting ??= new EmailSettingModel();

            var client = new SmtpClient();
            client.Connect(emailSetting.Host, emailSetting.Port, false);
            client.Authenticate(emailSetting.UserName, emailSetting.Password);


            var message = new MimeMessage();

            message.From.Add(new MailboxAddress(emailSetting.EmailFromName, emailSetting.EmailAddress));
            message.To.Add(new MailboxAddress(user.UserName, email));
            message.Subject = "Reset your password";

            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"Please reset your password by <a href='{resetLink}'>clicking here</a>."
            };

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
