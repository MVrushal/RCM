using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class ContactDetailsDto : BaseModel
    {
        public string Name { get; set; }
        public string Department { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public bool IsView { get; set; }
    }
}
