using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.BaseInterface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface.ClientAdmin
{
   public interface IUserGroupServices
   : IClientAdminGenericService<UserGroup>
    {

        Task<List<UserGroupDto>> GetUserGroupList(string ConnectionString, SqlParameter[] paraObjects);
        List<UserGroup> GetUserGroupListForDropDown(string ConnectionString,long BranchId);
    }
}
