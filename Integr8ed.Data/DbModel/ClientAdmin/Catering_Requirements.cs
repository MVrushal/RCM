using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Catering_Requirements : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public long CatererId { get; set; }
        [ForeignKey("CatererId")]
        public virtual Catering_Details Catering_Details { get; set; }

        public string Notes { get; set; }
        public string TimeFor { get; set; }
        public string TimeCollected { get; set; }
        public int NumberOfPeople { get; set; }
        public decimal Cost { get; set; }
    }
}
