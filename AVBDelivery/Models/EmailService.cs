using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace AVBDelivery.Models
{
    public class EmailService
    {
        public async Task SendEmailAsync(string email, string subject, string message, string userId)
        {
            var builder = new ConfigurationBuilder();
            // установка пути к текущему каталогу
            builder.SetBasePath(Directory.GetCurrentDirectory());
            // получаем конфигурацию из файла appsettings.json
            builder.AddJsonFile("appsettings.json");
            // создаем конфигурацию
            var config = builder.Build();
            //// получаем строку подключения
           

            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress(config.GetValue("MailFrom", "AVB Delivery"), config.GetValue("MailLogin", "")));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = message
            };

            using (var client = new SmtpClient())
            {
                try
                {
                    await client.ConnectAsync(config.GetValue("MailSMTP", ""), config.GetValue("MailPort", 465), true);
                }
                catch (System.Exception ex)
                {
                    await DBConnector.DBLogs.Error($"Не удалось подключиться к SMTP-серверу\n{ex.Message}\n{ex.InnerException}", userId);
                }
                try
                {
                    await client.AuthenticateAsync(config.GetValue("MailLogin", ""), config.GetValue("MailPwd", ""));
                }
                catch (System.Exception ex)
                {
                    await DBConnector.DBLogs.Error($"Не удалось провести аутентификацию SMTP-сервера\n{ex.Message}\n{ex.InnerException}", userId);
                }
                try
                {
                    await client.SendAsync(emailMessage);

                }
                catch (System.Exception ex)
                {
                    await DBConnector.DBLogs.Error($"Не удалось отправить письмо через SMTP-сервер\n{ex.Message}\n{ex.InnerException}", userId);

                }
                try
                {
                    await client.DisconnectAsync(true);

                }
                catch (System.Exception ex)
                {
                    await DBConnector.DBLogs.Error($"Не удалось отключиться от SMTP-сервера\n{ex.Message}\n{ex.InnerException}", userId);

                }

            }
        }
    }
}
