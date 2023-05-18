using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class Catering_Details : EntityWithAudit
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string CatererName { get; set; }

        public string  ContactName { get; set; }

        public string Telephone  { get; set; }
        public string Email { get; set; }
        public string FaxNumber  { get; set; }
        public string Address { get; set; }

        public string PostCode { get; set; }
        public long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }
    }
}
