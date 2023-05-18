using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class BookingStatusDto:BaseModel
    {
        public string Status { get; set; }
        public string ColorCode { get; set; }
        public bool IsView { get; set; }
    }
}
