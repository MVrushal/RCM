using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Cat_Req_Menu : EntityWithAudit
    {
        public long? CatererMenuId { get; set; }
        [ForeignKey("CatererMenuId")]
        public virtual CatererMenu CatererMenu { get; set; }

        public long Cat_ReqId { get; set; }
        [ForeignKey("Cat_ReqId")]
        public virtual Catering_Requirements Catering_Requirements { get; set; }
    }
}
