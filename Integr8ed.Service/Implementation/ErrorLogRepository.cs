using Integr8ed.Service.Interface;
using Integr8ed.Data;
using Integr8ed.Data.DbModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Integr8ed.Service.BaseService;

namespace Integr8ed.Service.Implementation
{
    public class ErrorLogRepository : GenericRepository<ErrorLog>, IErrorLogService
    {
        private readonly ApplicationDbContext _db;
        public ErrorLogRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public void AddErrorLog(System.Exception ex, string appType)
        {
            try
            {
                if (ex != null)
                { 
                var errorlog = new ErrorLog
                {
                    Source = ex.Source,
                    Path = appType,
                    TargetSite = ex.TargetSite.Name,
                    Type = ex.GetType().Name,
                    Message = ex.Message,// + entityValidationError,
                    Stack = ex.StackTrace,
                    LogDate = DateTime.UtcNow
                };
                Add(errorlog);
                Save();
                }
            }
            catch (System.Exception e)
            {
               // WriteToLogFile(e.Message);
                throw;
            }

        }


        private static void WriteToLogFile(string message)
        {
            var path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine("------------------------------------------------------------------");
                    sw.WriteLine(DateTime.UtcNow.ToShortDateString());
                    sw.WriteLine("Error in Error Log");
                    sw.WriteLine(message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine("------------------------------------------------------------------");
                    sw.WriteLine(DateTime.UtcNow.ToShortDateString());
                    sw.WriteLine("Error in Error Log");
                    sw.WriteLine(message);
                }
            }
        }
    }
}
