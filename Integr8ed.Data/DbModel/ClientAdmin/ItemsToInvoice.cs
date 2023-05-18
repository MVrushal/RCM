using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class ItemsToInvoice : EntityWithAudit
    {

        public long? InvoiceDetailsId { get; set; }
        [ForeignKey("InvoiceDetailsId")]
        public virtual InvoiceDetail InvoiceDetails { get; set; }

        public long? InvoiceMasterId { get; set; }
        [ForeignKey("InvoiceMasterId")]
        public virtual Invoice Invoice { get; set; }

        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public int Quantity { get; set; }
    }
}
