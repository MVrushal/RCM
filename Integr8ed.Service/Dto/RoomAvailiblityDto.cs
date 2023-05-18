using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class RoomAvailiblityDto : BaseModel
    {
        public string Title { get; set; }
        public string BookingDate { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Day { get; set; }
        public string b_StartTime { get; set; }
        public string b_FinishTime { get; set; }


    }
}
