using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class SecurityDto : BaseModel
    {
        public string DateCollected { get; set; }
        public string DateReturned { get; set; }
        public string TimeCollected { get; set; }
        public string TimeReturned { get; set; }
        public string CollectedBy { get; set; }
        public string ReturnedBy { get; set; }
        public string SecurityNotes { get; set; }
        public bool IsView { get; set; }
    }
}
