using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
  public  class Users : EntityWithAudit
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Frgt_Code { get; set; }
        public bool? IsAdmin { get; set; }
        [StringLength(50)]
        public string FirstName { get; set; }

        [StringLength(50)]
        public string MiddleName { get; set; }


        [StringLength(50)]
        public string LastName { get; set; }

        [NotMapped] public string FullName => $@"{FirstName} {LastName}";


        [Required, StringLength(20)]
        public string MobileNumber { get; set; }

        [StringLength(20)]
        public string TelephoneNumber { get; set; }

        public string Notes { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string UserImage { get; set; }
        
        public long? BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }

        public string AdminBranchId { get; set; }

    }
}
