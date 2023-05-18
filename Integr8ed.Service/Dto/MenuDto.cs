using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class MenuDto : BaseModel
    {
        public string Notes { get; set; }
        public string DescriptionOFFood { get; set; }
        public bool IsView { get; set; }
    }
}
