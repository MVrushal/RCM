using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
  public class CallLogs : EntityWithAudit
    {
        public string Subject { get; set; }

        public int EntryType { get; set; }
        public DateTime DateOfentry { get; set; }

        public  string Time { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string Comments { get; set; }
        public string PostCode { get; set; }
        public string TakenBy { get; set; }
        public string TakenFor { get; set; }
        public DateTime NextContactDate { get; set; }
        public bool ISCompleted { get; set; }   
        public int Associated { get; set; }

        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

    }
}
