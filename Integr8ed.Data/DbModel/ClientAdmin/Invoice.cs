using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Invoice : EntityWithAudit
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }

        public decimal Vate { get; set; }
        public decimal IteamCost { get; set; }
        public string BudgetRate { get; set; }
        public bool IsIteamVatable { get; set; }

        public long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }

    }
}
