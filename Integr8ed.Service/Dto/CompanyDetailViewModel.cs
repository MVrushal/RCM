using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Integr8ed.Service.Dto
{
    public class CompanyDetailViewModel : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [MinLength(4), MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z''-'\s]{4,20}$",
         ErrorMessage = "Name must be 4 to 20 characters")]
        public string Name { get; set; }

        [Required]
        [MinLength(4), MaxLength(20)]
        [RegularExpression(@"^[a-zA-Z''-'\s]{4,20}$",
         ErrorMessage = "SurName must be 4 to 20 characters")]
        public string SurName { get; set; }

        [Required]
        [MinLength(10), MaxLength(50)]
        [RegularExpression(@"^[a-zA-Z''-'\s]{10,50}$",
         ErrorMessage = "OrganisationName must be 10 to 50 characters")]
        public string OrganisationName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Address { get; set; }

        [Required]
        //[MinLength(6), MaxLength(6)]
        // [RegularExpression(@"^\d$",
        // ErrorMessage = "PostCode must be 6 characters")]
        public int PostCode { get; set; }

        [Required]
        // [MinLength(10), MaxLength(12)]
        //[RegularExpression(@"^\d$",
        //ErrorMessage = "Telephone must be 10 to 12 characters")]
        public int Telephone { get; set; }

        public List<PersonTitleDto> PersonTitleDtos { get; set; }

        public int? PersonTitleId { get; set; }
    }
}
