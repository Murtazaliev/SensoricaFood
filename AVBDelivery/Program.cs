using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AVBDelivery.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace AVBDelivery
{
    public class Program
    {
        public static NLog.Logger NLogger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        public static string appVersion = typeof(Program).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version;

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                
                try
                {                    
                    var userManager = services.GetRequiredService<UserManager<User>>();
                    var rolesManager = services.GetRequiredService<RoleManager<IdentityRole>>();
                    var context = services.GetService<ApplicationContext>();

                    await context.Database.MigrateAsync();
                    await RoleInitializer.InitializeAsync(userManager, rolesManager, context);
                }
                catch (Exception ex)
                {
                    NLogger.Error(ex, $"��������� ������ ��� �������������� ���������� �� ��������������.\n{ex.Message}\n{ex.InnerException}");
                }

                
            }



            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
