using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EasyLearnerAdmin.Data.DbModel.BaseModel
{
    public class EntityWithAudit
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public long? ModifiedBy { get; set; }

        public DateTime? ModifiedDate { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }
        [DefaultValue(false)]

        public bool IsDelete { get; set; }

    }
}
