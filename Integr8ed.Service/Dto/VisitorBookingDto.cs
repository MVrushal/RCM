using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class VisitorBookingDto : BaseModel
    {
        public long BookingDetailId { get; set; }
        public long VisitorId { get; set; }
        public bool IsView { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string SurName { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Mobile { get; set; }
        public string Notes { get; set; }
    }
}
