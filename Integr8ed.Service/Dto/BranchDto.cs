using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
  public  class BranchDto : BaseModel
    {
        public string BranchName { get; set; }
        
        public bool IsView { get; set; }
    }
}
