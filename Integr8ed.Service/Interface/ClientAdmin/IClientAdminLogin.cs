using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Interface
{
   public interface IClientAdminLogin
    {

        public List<UserDto> Login(UserDto userDto ,  string Connectionsatring);
        public List<Users> ForgotPassword(UserDto userDto, string Connectionsatring);
        public Users UpdatedUserDetail(UserDto userDto, string Connectionsatring);
        
    }
}
