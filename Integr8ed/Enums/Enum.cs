using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace Integr8ed.Enums
{
    public class Enum
    {
        public enum NotificationType
        {
            error,
            success,
            warning,
            info
        }

        public enum Menu
        { 
            RoomTypes=1,
            EquipmentRequirement=2,
            UserAccess=3,
        }

        //public enum BookingStatus
        //{
        //    Confirmed = 1,
        //    Canceled = 2,
        //    Pending = 3,
        //    Reserved = 4,
        //    Deposit = 5,
        //    Paid = 6,
        //    Waiting = 7,
        //    [Description("Pre-Booking")]
        //    PreBooking = 8
        //}
    }
}
