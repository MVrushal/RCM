using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class EquipmentRequiredForBooking : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public long EquipmentRequiredId { get; set; }
        [ForeignKey("EquipmentRequiredId")]
        public virtual Equipment Equipment { get; set; }

        public int NoofItem { get; set; }
    }

    public class CheckEquipmetIsavailable
    
    {

        public string EquipmetName { get; set; }
        public int NoOfItemINBooking { get; set; }
        public int NoOfItemAvalable { get; set; }

    }
    }
