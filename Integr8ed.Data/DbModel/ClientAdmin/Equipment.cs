using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
   public  class Equipment : EntityWithAudit
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public long EquipId { get; set; }
    }
}
