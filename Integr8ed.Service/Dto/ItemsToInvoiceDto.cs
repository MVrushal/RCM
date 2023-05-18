using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class ItemsToInvoiceDto : BaseModel
    {
        public long? InvoiceDetailsId { get; set; }
        public long? InvoiceMasterId { get; set; }
        public long? BookingDetailId { get; set; }
        public int Quantity { get; set; }
        public bool IsViewForInvItem { get; set; }
        public virtual InvoiceDto Invoice { get; set; }
    }
}
