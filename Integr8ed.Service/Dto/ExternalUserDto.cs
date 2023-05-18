using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Dto
{
   public class ExternalUserDto
    {

        public long Id { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }

        public string ReturnUrl { get; set; }

        public string CompanyCode { get; set; }
        public string Password { get; set; }
    }
}
