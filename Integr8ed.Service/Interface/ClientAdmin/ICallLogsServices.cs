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
    public interface ICallLogsServices : IClientAdminGenericService<CallLogs>
    {
        Task<List<CallLogsDto>> GetCallLogsListForCatereringDetail(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<CallLogsDto>> GetCallLogsListForBookingDetail(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<CallLogsDto>> GetCallLogsListForReminder(string ConnectionString, SqlParameter[] paraObjects);
    }
}
