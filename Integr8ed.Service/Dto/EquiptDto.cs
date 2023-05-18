using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
 public   class EquiptDto : BaseModel
    {
        public string code { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsView { get; set; }
        public long EquipId { get; set; }
        public string StandardEquipmentName { get; set; }
    }
}
