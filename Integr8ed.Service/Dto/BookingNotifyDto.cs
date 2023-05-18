using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class BookingNotifyDto
    {
        public long Id { get; set; }
        public string TitleOfMeeting { get; set; }
        public DateTime BookingDate { get; set; }
        public string StartTime { get; set; }
        public string FinishTime { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string RoomTitle { get; set; }
        public int CurrentComCode { get; set; }
    }
}
