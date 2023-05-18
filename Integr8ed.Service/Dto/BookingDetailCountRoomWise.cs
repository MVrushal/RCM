using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
  public  class BookingDetailExternalcountWise
    {

        public string MonthName { get; set; }
        public int ExternalBokingCount { get; set; }
        public int id { get; set; }
    }
    public class BookingDetailCountRoomWiseMulti
    {

        public string RoomName { get; set; }
        public int jan { get; set; }

        public int feb { get; set; }
        public int march { get; set; }
        public int april { get; set; }
        public int may { get; set; }
        public int june { get; set; }
        public int july { get; set; }
        public int augest { get; set; }
        public int sep { get; set; }
        public int oct { get; set; }
        public int nov { get; set; }
        public int dece { get; set; }
     
    }
    public class BookingDetailCountMonthWise
    {

        public string MonthNames { get; set; }
        public int BookingCount { get; set; }

        public int id { get; set; }
    }
    public class bookingDetailCountMeetingWises
    {

        public string MeetingName { get; set; }
        public int BokingCount { get; set; }
    }
    public class ToolTip
    {

        public string BookingDate { get; set; }
        public string StartTime { get; set; }

        public string    EndDate { get; set; }

    }


    public class DashboardDto
    {
        
        public List<BookingDetailExternalcountWise> bookingDetailExternalcountWises { get; set; }
        public List<BookingDetailCountStatusWises> bookingDetailCountStatusWises { get; set; }
         public List<bookingDetailCountMeetingWises> bookingDetailCountMeetingWises { get; set; }
        public List<BookingDetailCountMonthWise> bookingDetailCountMonthWises { get; set; }
        public List<DasboardCount> AdminDasboardCount { get; set; }

        public List<BookingDetailCountRoomWiseMulti> bookingDetailCountRoomWiseMultis { get; set; }
    }
    public class DasboardCount
    {
        public int BookingCount { get; set; }
        public int CateringCount { get; set; }
        public int Visitors { get; set; }

        public int TotalEarning { get; set; }
    }
    public class BookingDetailCountStatusWises
    {

        public string status { get; set; }
        public int StatusCount { get; set; }
    }
    
}
