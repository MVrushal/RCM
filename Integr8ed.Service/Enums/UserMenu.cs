using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Integr8ed.Service.Enums
{
    public static class UserMenu
    {
    public enum Access
        {
            RoomType = 1,
            EquipmentRequirement = 2,
            DelegetsCodes = 3,
            MeetingType = 4,
            InvoiceItem = 5,
            EntryType = 6,
            UserGroup = 7,
            CateringDetail=8,
            RoomAvailability=9,
            Internal_ExternalBooking=10,
            RecurringBooking=11,
            RoomAvailibility =12,
            BookingDiary =13,
            Reports =14,
            ManageBranch =15,
            BookingStatus = 16,
            Denied = 0
        }
        public enum RequestStatus
        { 
            Pending=1,
            Accepted=2,
            Rejected=3

         }
    }
}
