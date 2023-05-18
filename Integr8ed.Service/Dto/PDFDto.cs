using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class PDFDto
    {
        public string BookingDate { get; set; }
        public string UserGroup { get; set; }
        public string Contact { get; set; }
        public string RoomType { get; set; }
        public string BookingStatus { get; set; }
        public string MeetingTitle { get; set; }
        public string NumberOfpeople { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public string Slot { get; set; }
        public string ReportTitle { get; set; }
        public string CompanyName { get; set; }

    }

    public class CompanyName
    {
        public string OrganizationName { get; set; }
    }
}
