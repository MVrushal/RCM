using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace Integr8ed.Service.Dto
{
   public class CompanyDto:BaseModel
    {


        public string OrganisationName { get; set; }

        public string Address { get; set; }

        public string PostCode { get; set; }

        public string Telephone { get; set; }

        public string CompanyCode { get; set; }

        public string FirstName { get; set; }

        public string Name { get; set; }

        public string SurName { get; set; }

        public string Email { get; set; }

        public string Password { get; set; }
        public string FullName { get; set; }

        public bool IsView { get; set; }

    }
}
