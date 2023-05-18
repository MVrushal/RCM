using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
   public class ReportsDTO
    {
        public long? roomTypeId { get; set; }
        public string startDate { get; set; }
        public string endDate { get; set; }
        public long? userGroupID { get; set; }
        public long? BookingStatus { get; set; }
        public string TitleOfMeeting { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }
       
    }
    public class ReportGridDTo
    {
        public int TotalRecords { get; set; }
        public long BookingNumber { get; set; }
        public DateTime BookingDate { get; set; }
        public string    StringBookingDate { get; set; }
        public string UserGroupName { get; set; }
        public string MeetingTitle { get; set; }
        public string RoomType { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public long BookingType { get; set; }
        public string BookingStatus { get; set; }

    }
}
