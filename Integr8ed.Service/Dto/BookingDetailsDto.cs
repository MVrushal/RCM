using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class BookingDetailsDto : BaseModel
    {
        public long UserGroupId { get; set; }
        public long MeetingTypeId { get; set; }
        public long RoomTypeId { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingDateS { get; set; }
        public string BookingDateForDisplay { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public string Duration { get; set; }
        public string TitleOfMeeting { get; set; }
        public int NumberOfAttending { get; set; }
        public int CarSpaceRequired { get; set; }
        public bool HouseKeepingRequired { get; set; }
        public bool SecurityRequired { get; set; }
        public decimal Cost { get; set; }
        public string LayoutInformation { get; set; }
        public bool TobeInvoiced { get; set; }
        public bool TechnicianOnSite { get; set; }
        public bool DisabledAccess { get; set; }
        public bool ReturnedBookingForm { get; set; }
        public string BookingContact { get; set; }
        public string Mobile { get; set; }
        public string DateFromSent { get; set; }
        public int BookingStatus { get; set; }
        public string AdditionalInformation { get; set; }
        public string CancellationDetail { get; set; }
        public bool IsView { get; set; }
        public bool IsInternalBooking { get; set; }
        public long? ExternalBookingClientId { get; set; }
        public string CatererRemark { get; set; }
        public long ExternalUserId { get; set; }
        public long BranchId { get; set; }
        public long RequestId { get; set; }

        public string Email { get; set; }
        public string Slot { get; set; }
        public string CreatedUserName { get; set; }
        public string BookingCreatedDate { get; set; }
      

        //invoice fields
        public string RoomType { get; set; }
        public string  HourlyRate { get; set; }
        public string  SaturdayRate { get; set; }
        public string  SundayRate { get; set; }
        

        public int NotifyDays { get; set; }
        //Booking Details field
        public string branchName { get; set; }
        //
        public long selectedFromDiaryId { get; set; }

    }






    public class BookingDetailsForGridDto : BaseModel
    {
        public long Id { get; set; }
        public string UserGroupName { get; set; }
        public string MeetingTitle { get; set; }
        public string MeetingType { get; set; }
        public string RoomTitle { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public int NumberOfAttending { get; set; }
        public int CarSpaceRequired { get; set; }

        public bool HouseKeepingRequired { get; set; }

        public bool SecurityRequired { get; set; }

        public decimal Cost { get; set; }
        public string LayoutInformation { get; set; }

        public bool TobeInvoiced { get; set; }
        public bool TechnicianOnSite { get; set; }
        public bool DisabledAccess { get; set; }
        public bool ReturnedBookingForm { get; set; }

        public string Mobile { get; set; }
        public string DateFromSent { get; set; }
        public DateTime BookingDate { get; set; }
        public string BookingDateForDisplay { get; set; }
        public string BookingContact { get; set; }

        public int BookingStatus  { get; set; }

        public string AdditionalInformation { get; set; }

        public string CancellationDetail { get; set; }


        public string BookingStatusName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public long? ExternalBookingClientId { get; set; }
        public string CreatedUserName { get; set; }
        public string BookingCreatedDate { get; set; }
    }
}
