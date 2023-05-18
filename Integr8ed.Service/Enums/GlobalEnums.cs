using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Integr8ed.Service.Enums
{
    public class GlobalEnums
    {
        //Global enum  will be here
        public enum BookingStatus
        {
            Confirmed = 1,
            Cancelled = 2,
            Pending = 3,
            Reserved = 4,
            Deposit = 5,
            Paid = 6,
            Waiting = 7,
            [Description("Pre-Booking")]
            PreBooking = 8
        }

        public enum AssociatedCallLog
        {
            BookingDetail = 1,
            CateringDetail = 2
        }

        public enum NameOfDays
        {
            Monday = 1,
            Tuesday = 2,
            Wednesday = 3,
            Thursday = 4,
            Friday = 5,
            Saturday = 6,
            Sunday = 7
        }

       
    }
}
