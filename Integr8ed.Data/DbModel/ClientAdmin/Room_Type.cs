using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
   public class Room_Type :  EntityWithAudit
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int Floor { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal SundayRate { get; set; }
        public decimal SaturdayRate { get; set; }
        public int Maxperson { get; set; }

        public long BranchId { get; set; }
        public int RoomOrder { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }

    }


    public class DayViewReportDTO
    {
        public string BookingDate { get; set; }
        public string startTime { get; set; }
        public string finishTime { get; set; }
        public long BookingId { get; set; }
    }


}
