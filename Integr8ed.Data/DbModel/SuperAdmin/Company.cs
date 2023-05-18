using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.SuperAdmin
{
   public class Company : EntityWithAudit
    {

        [Required]
       
        public string OrganisationName { get; set; }

     
       
        public string Address { get; set; }

        public string PostCode { get; set; }

       
       
        public string Telephone { get; set; }


        [Required]
        public string CompanyCode { get; set; }

        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser ApplicationUser { get; set; }

    }
}
