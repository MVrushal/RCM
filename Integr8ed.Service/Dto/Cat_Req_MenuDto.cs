using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class Cat_Req_MenuDto : BaseModel
    {
        public long? CatererMenuId { get; set; }
        public long Cat_ReqId { get; set; }
    }
}
