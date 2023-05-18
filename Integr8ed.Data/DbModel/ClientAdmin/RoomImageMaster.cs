using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class RoomImageMaster
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }


        public long RoomId { get; set; }
        [ForeignKey("RoomId")]
        public virtual Room_Type Room_Type{ get; set; }

        public string RoomImage { get; set; }
    }
}
