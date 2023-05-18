using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel
{
   public class ErrorLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long LogId { get; set; }
        public string Source { get; set; }
        public string TargetSite { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Stack { get; set; }
        public DateTime LogDate { get; set; }
        public string Path { get; set; }

        public static void AddErrorLog(Exception ex, string v)
        {
            throw new NotImplementedException();
        }
    }
}
