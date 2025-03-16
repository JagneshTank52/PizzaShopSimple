using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using PizzaShop.Entity.ViewModels.HelperVM;
using PizzaShop.Service.Interface;

namespace PizzaShop.Service.Implementaion;

public class EmailService : IEmailService
{
     private readonly EmailSettingVM _emailSettings;

    public EmailService(IOptions<EmailSettingVM> emailSettings){
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body){
       
            using var client = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                Credentials = new NetworkCredential(_emailSettings.Username,_emailSettings.Password),
                EnableSsl = _emailSettings.EnableSsl
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            message.To.Add(to);
        try
        {
            await client.SendMailAsync(message);
            return true;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Email Sending Failed: {ex.Message}");
            return false;
        }
    }
}
