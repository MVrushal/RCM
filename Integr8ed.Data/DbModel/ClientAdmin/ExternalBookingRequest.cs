using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
   public class ExternalBookingRequest
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        public long ExternalUserId { get; set; }
        [ForeignKey("ExternalUserId")]
        public virtual Users Users { get; set; }

        public long BookingId { get; set; }
        [ForeignKey("BookingId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public bool IsAccepted { get; set; }

    }
}
