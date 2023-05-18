using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class ContactDetails : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public string Name { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
    }
}
