using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
   public class UserRoles
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual Users Users { get; set; }

        public long RoleId { get; set; }
        [ForeignKey("RoleId")]
        public virtual Roles Roles { get; set; }

    }
}
