using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AVBDelivery.Interfaces;
using AVBDelivery.Jobs;
using AVBDelivery.Models;
using AVBDelivery.Services;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using Serilog;
using StackExchange.Redis;

namespace AVBDelivery
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // ��������� ������ ���������� ������
            services.AddTransient<IPasswordValidator<User>,
                    CustomPasswordValidator>(serv => new CustomPasswordValidator(6));

            // ��������� ����� ������������
            services.AddTransient<IUserValidator<User>, CustomUserValidator>();
            
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"))
                
                );

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost";
                options.InstanceName = "delivery";
            });

            services.AddIdentity<User, IdentityRole>(opts => {
                opts.User.RequireUniqueEmail = true;    // ���������� email
                opts.User.AllowedUserNameCharacters = ".@abcdefghijklmnopqrstuvwxyz1234567890"; // ���������� �������
            })
            .AddEntityFrameworkStores<ApplicationContext>()
            .AddDefaultTokenProviders(); ;

            services.AddControllersWithViews();
            services.AddRazorPages();
            services.AddServerSideBlazor();

            services.AddHttpClient();

            //services.AddSingleton<NLog.Logger>();

            services.AddLogging(
                l =>
                {
                    l.ClearProviders();
                    Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).CreateLogger();
                    l.AddSerilog();
                }
            );
           

            services.AddSingleton<ILoggerProvider, NLogLoggerProvider>();
            services.AddTransient<INomenclatureUploader, NomenclatureUploader>();

            services.AddTransient<IIikoTransport, IIkoTransport>();
            services.AddTransient<AmoCrm>();

            services.AddSystemd();

            services.AddQuartz(
                c => c
                    .AddJob<NomenclatureUploaderJob>(o => o.WithIdentity("NomenclatureUploader"))
                    .AddTrigger(o => o.ForJob("NomenclatureUploader")
                        .WithIdentity("NomenclatureUploader-trigger")
                        .WithCronSchedule(Configuration["Quartz:NomenclatureUploader"] ?? "0 0 9 * * ?")
                        .UsingJobData("apiKey", Configuration["TransportApi:ApiKey"])
                        .StartNow()
                    )

            );
            services.AddQuartzHostedService(o => o.WaitForJobsToComplete = true);

            services.AddSingleton<IConnectionMultiplexer>(sp =>
            {
                var config = ConfigurationOptions.Parse(
                    Configuration.GetConnectionString("Redis"),
                    true);

                config.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(config);
            });

            services.AddMemoryCache();

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();    // ����������� ��������������
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
               endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
