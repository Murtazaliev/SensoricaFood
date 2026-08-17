using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AVBDelivery.Models
{
    public class RoleInitializer
    {
        public static NLog.Logger NLogger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();


        public static async Task InitializeAsync(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, ApplicationContext context)
        {

            // Создаём пользователей
            string adminEmail = "admin@example.com";
            string password = "uVQ5E7wzgzyWV3pInWMp";
            if (await roleManager.FindByNameAsync("admin") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("admin"));
                NLogger.Info("Создана роль admin.");
            }
            if (await roleManager.FindByNameAsync("client") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("client"));
                NLogger.Info("Создана роль client.");
            }
            if (await roleManager.FindByNameAsync("operator") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("operator"));
                NLogger.Info("Создана роль operator.");
            }
            if (await roleManager.FindByNameAsync("integrator") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("integrator"));
                NLogger.Info("Создана роль integrator.");
            }
            if (await roleManager.FindByNameAsync("nomenclatureEditor") == null)
            {
                await roleManager.CreateAsync(new IdentityRole("nomenclatureEditor"));
                NLogger.Info("Создана роль nomenclatureEditor.");
            }
            if (await userManager.FindByNameAsync(adminEmail) == null)
            {
                User admin = new User { Email = adminEmail, UserName = adminEmail, EmailConfirmed = true };
                IdentityResult result = await userManager.CreateAsync(admin, password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "admin");
                    await userManager.AddToRoleAsync(admin, "integrator");
                    await userManager.AddToRoleAsync(admin, "client");
                }
                var contact = new Contact
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = "denvic",
                    UserId = admin.Id
                };
                await context.Contacts.AddAsync(contact);
                NLogger.Info("Создан первоначальный пользователь с ролью admin.");

            }

            //Создаём шаблон письма
            if ((await context.EmailTemplates.FirstOrDefaultAsync()) == null)
            {
                NLogger.Info("Создаём типовые шаблоны письма.");
                var t = Environment.CurrentDirectory;
                var tt = Directory.GetCurrentDirectory();
                string path = "./init/email/register.txt";
                // асинхронное чтение
                try
                {
                    using (StreamReader reader = new StreamReader(path))
                    {
                        string text = await reader.ReadToEndAsync();
                        await context.EmailTemplates.AddAsync(new EmailTemplate() { Name = "Register", Template = text });
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    NLogger.Error($"Типовые шаблоны письма создать не удалось.\n{ex.Message}\n{ex.InnerException}");

                }

            }

        }
    }
}
