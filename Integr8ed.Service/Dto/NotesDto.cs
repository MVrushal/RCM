using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class NotesDto : BaseModel
    {
        public string Note { get; set; }
        public bool IsView { get; set; }
    }
}
