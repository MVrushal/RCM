using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Security : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public string DateCollected { get; set; }
        public string DateReturned { get; set; }
        public string TimeCollected { get; set; }
        public string TimeReturned { get; set; }
        public string CollectedBy { get; set; }
        public string ReturnedBy { get; set; }
        public string SecurityNotes { get; set; }
    }
}
