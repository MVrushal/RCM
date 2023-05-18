using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class DatabaseParamDto
    {
        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string DataFileName { get; set; }
        public string DataPathName { get; set; }
        public string DataFileGrowth { get; set; }
        public string LogFileName { get; set; }
        public string LogPathName { get; set; }

        public string LogFileGrowth { get; set; }

        public string UserID { get; set; }
        public string Password { get; set; }
    }
}
