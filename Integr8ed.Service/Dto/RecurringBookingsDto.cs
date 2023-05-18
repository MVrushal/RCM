using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class RecurringBookingsDto : BaseModel
    {
        public long? ExternalBookingClientId { get; set; }
        public int BookingType { get; set; }
        public string EventTitle { get; set; }
        public string DateFrom { get; set; }
        public string DateTo { get; set; }

        public int Every { get; set; }
        public string TimeStart { get; set; }
        public string TimeEnd { get; set; }

        public int BookingStatus { get; set; }
        public int NumberOfAttending { get; set; }
        public int CarSpaceRequired { get; set; }
        public string BookingContact { get; set; }
        public string Mobile { get; set; }

        public long UserGroupId { get; set; }
        public long RoomTypeId { get; set; }
        public bool IsView { get; set; }

        public string NameofDays { get; set; }
        public string UserName { get; set; }
        public string RoomTypeName { get; set; }

        public DateTime BookingDate { get; set; }
        public string bookingDateRecurring { get; set; }
        public string BookingDateForDisplay { get; set; }

        public long MeetingTypeId { get; set; }
        public string    BookingDay { get; set; } 
    }

    public class NoofDatesForBooking
    {
        public List<string> DateTimes { get; set; }
        public int Count { get; set; }
    }

    public class RecurringBookingsList
    {
        public long Id { get; set; }
        public string RoomTitle { get; set; }
        public string BookingDate { get; set; }
        public string Status { get; set; }
        public string StartTime{ get; set; }
        public string FinishTime{ get; set; }

        public string ReportTitle { get; set; }
    }

}
