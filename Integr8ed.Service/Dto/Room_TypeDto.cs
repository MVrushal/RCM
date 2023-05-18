using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
   public class Room_TypeDto:BaseModel
    {
        public string code { get; set; }
        public string RoomTitle { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public int Floor { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal SundayRate { get; set; }
        public decimal SaturdayRate { get; set; }
        public int Maxperson { get; set; }
        public bool IsView { get; set; }
        public int RoomOrder { get; set; }

        public string[] roomImageList { get; set; }
    }

    public class RoomTypeImageDto
    {
        public long Id { get; set; }
        public long RoomId { get; set; }
        public IFormFile[] RoomImage { get; set; }

        public string ImageName { get; set; }

        public List<string> roomImagesList { get; set; }

      public  string path { get; set; }

    }
}
