using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class UpdateDiaryDto : BaseModel
    {
        public long RoomTypeId { get; set; }
        public long BookingId { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string BookingDate { get; set; }
    }
}
