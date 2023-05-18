using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Integr8ed.Service.Dto
{
   public class UserDto
    {
        public long ID { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string NewPassword { get; set; }
        public string Email { get; set; }
        public string Frgt_Code { get; set; }
        public bool IsAdmin { get; set; }
       
        public string FirstName { get; set; }
       
        public string MiddleName { get; set; }
       
        public string LastName { get; set; }

        public string FullName { get; set; }

        public string MobileNumber { get; set; }
    
        public string TelephoneNumber { get; set; }

        public string Notes { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public long TotalRecords { get; set; }

        public bool  IsActvie { get; set; }
        
        public List<int> MenuId { get; set; }
        public string CompanyCode { get; set; }

        public string UserImage { get; set; }

        public bool IsView { get; set; }

        public IFormFile UserImageFile { get; set; }

        public bool IsEmailEdit { get; set; }
        public long  BranchId { get; set; }
        public string  Role { get; set; }
        public long  RoleId { get; set; }
        public string AdminBranchId { get; set; }
    }


}
