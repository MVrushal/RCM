using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
   public class UserAccessDto
    {
        public long UserId { get; set; }
        public bool RoomType { get; set; }
        public bool EquipmentRequirment { get; set; }
        public bool UserGroup { get; set; }
        public bool MeetingType { get; set; }
        public bool DelegetsCodes { get; set; }
        public bool EntryType { get; set; }
        public bool InvoiceItem { get; set; }
        public bool CateringDetail { get; set; }
        public bool RoomAvailability { get; set; }
        public bool Internal_ExternalBooking { get; set; }
        public bool RecurringBooking { get; set; }
        public bool ManageBranch { get; set; }
        public bool BookingDiary { get; set; }
        public bool Reports { get; set; }

        public bool BranchAdmin { get; set; }
        public string BranchListID { get; set; }
        public long BranchId { get; set; }
        public bool IsAdmin { get; set; }

    }


}
