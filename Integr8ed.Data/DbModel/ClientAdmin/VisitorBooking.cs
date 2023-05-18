using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class VisitorBooking : EntityWithAudit
    {
        public long BookingDetailId { get; set; }
        public long VisitorId { get; set; }
    }
}
