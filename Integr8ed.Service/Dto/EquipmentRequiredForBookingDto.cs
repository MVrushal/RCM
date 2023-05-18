using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class EquipmentRequiredForBookingDto : BaseModel
    {
        public long BookingDetailId { get; set; }
        public long EquipmentRequiredId { get; set; }
        public int NoofItem { get; set; }
        public string EqupTitle { get; set; }
        public string Description { get; set; }
        public long EquipId { get; set; }
        public bool IsView { get; set; }
    }
}
