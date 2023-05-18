using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service
{
    public class UserRoles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string ClientAdmin = "ClientAdmin";
        public const string BranchAdmin = "BranchAdmin";
        public const string Staff = "Staff";
    }
    enum UserRolesEnum
    {
        ClientAdmin = 1,
        BranchAdmin = 2,
        User = 3,
        ExternalUser = 4
    }
    enum Level
    {
        Low,
        Medium,
        High
    }
    public class EmailTemplateList
    {
        public const string EmailTemplate = @"EmailTemplate\";
    }

    public class RoomColors
    {
        public const string Confirmed = "color_one";
        public const string PreBooking = "color_two";
        public const string Paid = "color_three";
        public const string Deposit = "color_four";
        public const string Canceled = "color_five";
        public const string Pending = "color_six";
        public const string Reserved = "color_seven";
        public const string Waiting = "color_eight";
    }

    public class StoredProcedureList
    {
        public const string GetDBwiseBookingNotifyList = @"GetDBwiseBookingNotifyList";
    }

}
