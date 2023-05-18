using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class RmAvParamDto 
    {
        public long roomTypeId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public int maxPerson { get; set; }
        public long userGroup { get; set; }
        public int BookingStatus { get; set; }
        public int NumberOfAttending { get; set; }
        public long ClientId { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
        public bool ExcludeSunday { get; set; }
        public bool ExcludeMonday { get; set; }
        public bool ExcludeTuesday { get; set; }
        public bool ExcludeWednesday { get; set; }
        public bool ExcludeThursday { get; set; }
        public bool ExcludeFriday { get; set; }
        public bool ExcludeSaturday { get; set; }

        public string BookingDate { get; set; }

    }
}
