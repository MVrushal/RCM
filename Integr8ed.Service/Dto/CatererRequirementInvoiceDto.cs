using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class CatererRequirementInvoiceDto : BaseModel
    {

        public long BookingDetailId { get; set; }
        public string CatererName { get; set; }
        public int   NoOfPeople { get; set; }
        public string   Menu { get; set; }
        public decimal PricePerItem { get; set; }

        public string BookingContact { get; set; }
        public string BookingDate { get; set; }



    }




}
