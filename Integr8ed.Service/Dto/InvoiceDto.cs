using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class InvoiceDto : BaseModel
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Vate { get; set; }
        public decimal IteamCost { get; set; }
        public string BudgetRate { get; set; }
        public bool IsIteamVatable { get; set; }
        public bool IsView { get; set; }
        public int Quantity { get; set; }
    }
}
