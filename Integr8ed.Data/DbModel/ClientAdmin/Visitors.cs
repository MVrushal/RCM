using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Visitors : EntityWithAudit
    {
        public string Description { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Notes { get; set; }

        public long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }
    }
}
