using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System;

namespace AVBDelivery.Models
{
    public class DBConnector
    {
        static NLog.Logger NLogger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

        //static DbContextOptionsBuilder optionsBuilder;

        static DbContextOptions<ApplicationContext> options;

        static public void CreateOptions()
        {

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationContext>();

            var settings = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            string SQLVendor = settings.GetSection("SQLVendor").Value;
            string DefaultConnection = settings.GetSection("ConnectionStrings")["DefaultConnection"];

            options = optionsBuilder
                        .UseSqlServer(DefaultConnection)
                        .Options;



        }

        public DBConnector()
        {
            CreateOptions();
        }

        #region Логирование в БД

        public static class DBLogs
        {
            static public async Task Info(string message, string user = "", string additionalInfo = "")
            {
                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Information, user, additionalInfo);
            }

            static public async Task Critical(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Critical, user, additionalInfo);
            }

            static public async Task Debug(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Debug, user, additionalInfo);
            }

            static public async Task Error(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Error, user, additionalInfo);
            }

            static public async Task None(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.None, user, additionalInfo);
            }

            static public async Task Trace(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Trace, user, additionalInfo);
            }

            static public async Task Warning(string message, string user = "", string additionalInfo = "")
            {
                var stacktrace = new StackTrace();
                var prevframe = stacktrace.GetFrame(1);
                var method = prevframe.GetMethod();

                await UpdateLogDB(message, Microsoft.Extensions.Logging.LogLevel.Warning, user, additionalInfo);
            }

            public static async Task<List<DBLog>> Get(List<DBLog> model)
            {
                try
                {
                    using (ApplicationContext db = new ApplicationContext(options))
                    {
                        // получаем объекты из бд
                        model = await db.DBLog.ToListAsync();
                    }
                }
                catch (Exception e)
                {
                    NLogger.Error("При получении журнала событий возникла ошибка.\n\r" + e.Message);
                }
                return model;
            }


            static async Task UpdateLogDB(string message, Microsoft.Extensions.Logging.LogLevel logLevel, string user, string additionalInfo)
            {
                
                DBLog dBLog = new DBLog
                {
                    DateTime = DateTime.Now,
                    Level = logLevel.ToString(),
                    Message = message,
                    User = user,
                    AdditionalInfo = additionalInfo
                };

                try
                {
                    using (ApplicationContext db = new ApplicationContext(options))
                    {

                        await db.DBLog.AddAsync(dBLog);
                        await db.SaveChangesAsync();
                    }
                }
                catch (Exception e)
                {
                    NLogger.Error("При записи истории событий в БД возникла ошибка.\n\r" + e.Message + "\n\r" + $"  DateTime={dBLog.DateTime}\n\r   Level = {dBLog.Level}\n\r   Message = {dBLog.Message}\n\r   User = {dBLog.User}\n\r AdditionalInfo = {dBLog.AdditionalInfo}\n\r");
                }
            }

        }

        #endregion

    }
}
