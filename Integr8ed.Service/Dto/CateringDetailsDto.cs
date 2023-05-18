using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class CateringDetailsDto : BaseModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string CatererName { get; set; }
        public string ContactName { get; set; }
        public string Telephone { get; set; }
        public string Email { get; set; }
        public string FaxNumber { get; set; }
        public string Address { get; set; }
        public string PostCode { get; set; }
        public bool IsView { get; set; }
        public bool IsEmailEdit { get; set; }
    }
}
