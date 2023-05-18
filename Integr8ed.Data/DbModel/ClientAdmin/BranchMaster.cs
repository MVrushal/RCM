using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
  public class BranchMaster:EntityWithAudit
    {
      
        public string BranchName { get; set; }

        public virtual ICollection<BookingDetails> BookingDetails { get; set; }

    }
}
