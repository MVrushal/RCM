using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
   public class UserAccess : EntityWithAudit
    {

        public long UserId { get; set; }
        public long MenuId { get; set; }
    }
}
