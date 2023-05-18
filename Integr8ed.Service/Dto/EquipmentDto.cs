using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class EquipmentDto : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsView { get; set; }
    }
}
