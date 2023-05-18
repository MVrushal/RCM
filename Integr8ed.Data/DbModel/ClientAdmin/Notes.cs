using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Notes : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public string Note { get; set; }
    }
}
