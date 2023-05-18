using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.BaseInterface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface
{
    public interface IReportsService : IClientAdminGenericService<ReportGridDTo>
    {

        Task<List<ReportGridDTo>> GetReportList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<ReportGridDTo>> GetReportExcel(string ConnectionString, SqlParameter[] paraObjects);




    }
}
