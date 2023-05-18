using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class DiaryDto : BaseModel
    {
        public List<RoomTypeListDto> BookingList { get; set; }
    }
    public class RoomTypeListDto
    {
        public string StartTime { get; set; }
        public string FinishTime { get; set; }        
        public long RoomTypeId { get; set; }

        public string ColorCode { get; set; }

        public int BookingStatus { get; set; }

        public string Title { get; set; }

        public string  BookingDate { get; set; }

        public long BookingId { get; set; }

        public string UserGroupName { get; set; }
        public string MeetingTitle { get; set; }
        public string Contact { get; set; }
    }


    public class DiaryReportDTO
    {
        public string StartTime { get; set; }
        public string EndTime { get; set; }

        public string BookingDate { get; set; }

        public long BookingId { get; set; }
    }
}
