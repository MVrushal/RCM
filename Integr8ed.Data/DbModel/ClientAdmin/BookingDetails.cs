using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class BookingDetails : EntityWithAudit
    {
        public long UserGroupId { get; set; }
        [ForeignKey("UserGroupId")]
        public virtual UserGroup UserGroup { get; set; }

        public long? MeetingTypeId { get; set; }
        [ForeignKey("MeetingTypeId")]
        public virtual MeetingType MeetingType { get; set; }

        public long RoomTypeId { get; set; }
        [ForeignKey("RoomTypeId")]
        public virtual Room_Type Room_Type { get; set; }

        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
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
        public string Email{ get; set; }
        public string Mobile { get; set; }
        public string DateFromSent { get; set; }
        public int BookingStatus { get; set; }
        public string AdditionalInformation { get; set; }
        public string CancellationDetail { get; set; }

        public long? ExternalBookingClientId { get; set; }
        [ForeignKey("ExternalBookingClientId")]
        public virtual Users User { get; set; }

        public string CatererRemark { get; set; }

        public long BranchId { get; set; }
        [ForeignKey("BranchId")]
        public virtual BranchMaster BranchMaster { get; set; }

        public int NotifyDays { get; set; }

       
    }
}
