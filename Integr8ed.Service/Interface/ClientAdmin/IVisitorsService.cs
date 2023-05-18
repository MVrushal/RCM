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
    public interface IVisitorsService : IClientAdminGenericService<Visitors>
    {

        Task<List<VisitorDto>> GetVisitorList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<VisitorDto>> loadUserMobile(string ConnectionString, string mobile);
    }
}
