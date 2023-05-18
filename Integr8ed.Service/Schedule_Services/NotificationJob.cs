using ClosedXML.Excel;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Integr8ed.Data.DbContext;
using Integr8ed.Service.Enums;
using Integr8ed.Service.Interface;
using Quartz;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.Extensions.Configuration;

namespace Integr8ed.Service.Schedule_Services
{
    [DisallowConcurrentExecution]
    public class NotificationJob : IJob
    {
        private readonly ILogger<NotificationJob> _logger;
        private IConfiguration _config;
        public NotificationJob(ILogger<NotificationJob> logger, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
        }
        public Task Execute(IJobExecutionContext context)
        {
            _logger.LogInformation("Hello world!");


            var test = new LoadReport(_config);
            var isDownload = test.DownloadReport();

            return  Task.CompletedTask;
        }
    }
}
