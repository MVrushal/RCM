using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class ReportsRepository : ClientAdminGenericRepository<ReportGridDTo>, IReportsService
    {
        private readonly ClientAdminDbContext _context;
        public ReportsRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<ReportGridDTo>> GetReportList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetBookingReportTypes, paraObjects);
                    return Common.ConvertDataTable<ReportGridDTo>(dataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

        public async Task<List<ReportGridDTo>> GetReportExcel(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetReportExcel, paraObjects);
                    return Common.ConvertDataTable<ReportGridDTo>(dataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {

                throw ex;
            }
        }

    }
}
