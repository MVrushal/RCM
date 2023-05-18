using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class BookingStatus : EntityWithAudit
    {
        public string Status { get; set; }
        public string ColorCode { get; set; }
    }
}
