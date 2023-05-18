using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class CatererMenu : EntityWithAudit
    {
        public long? CatererId { get; set; }
        [ForeignKey("CatererId")]
        public virtual Catering_Details Catering_Details { get; set; }

        public long? MenuId { get; set; }
        [ForeignKey("MenuId")]
        public virtual Menu Menu { get; set; }

        public decimal Cost { get; set; }

       
    }
}
