using Integr8ed.Data;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.BaseInterface;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.BaseInterface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface
{
    public interface IUserService : IClientAdminGenericService<Users>
    {
        Task<List<Users>> GetUsersList(string ConnectionString, long BranchId);

        Task<List<Users>> GetUserName(string ConnectionString);

        Users GetCompanyAdmin(string ConnectionString, long BranchId);

        void   AddRoles(string ConnectionString,long AdminId);

        bool AddExternalUser(string ConnectionString, ExternalUserDto model);


    }
    public interface ISuparAdminUserService : IGenericService<ApplicationUser>
    {


    }

    
}
