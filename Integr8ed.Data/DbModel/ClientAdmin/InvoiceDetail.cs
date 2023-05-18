using EasyLearnerAdmin.Data.DbModel.BaseModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Integr8ed.Data.DbModel.ClientAdmin
{
    public class InvoiceDetail : EntityWithAudit
    {
        public long? BookingDetailId { get; set; }
        [ForeignKey("BookingDetailId")]
        public virtual BookingDetails BookingDetails { get; set; }

        public string InvoiceAddress { get; set; }
        public string InvoicePostCode { get; set; }
        public string ContactName { get; set; }
        public string InvoiceTo { get; set; }
        public string Mobile { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public decimal HireofFacilities { get; set; }
        public string CostCentreCode { get; set; }
        public string BudgetCode { get; set; }
        public decimal VatRate { get; set; }
        public decimal ProfitValue { get; set; }
        public string InvoiceNotes { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal GrossAmount { get; set; }
        public string InvoiceRequestDate { get; set; }     
    }

}
