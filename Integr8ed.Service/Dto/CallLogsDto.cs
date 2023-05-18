using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class CallLogsDto : BaseModel
    {
        public string Subject { get; set; }
        public int EntryType { get; set; }
        public string EntryTypeTitle { get; set; }
        public DateTime DateOfentry { get; set; }
        public string Time { get; set; }
        public string Contact { get; set; }
        public string Address { get; set; }
        public string Comments { get; set; }
        public string PostCode { get; set; }
        public string TakenBy { get; set; }
        public string TakenFor { get; set; }
        public DateTime NextContactDate { get; set; }
        public bool ISCompleted { get; set; }
        public bool IsView { get; set; }
        public string EntryDate { get; set; }
        public string NextconDate { get; set; }
        public int Associated { get; set; }
        public long? BookingDetailId { get; set; }
    }
}
