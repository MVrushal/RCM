using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class VisitorBookingRepository : ClientAdminGenericRepository<VisitorBooking>, IVisitorBookingServices
    {
        private readonly ClientAdminDbContext _context;
        public VisitorBookingRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<VisitorBookingDto>> GetVisitor_BookingList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetVisitorBookingList, paraObjects);
                    var test = Common.ConvertDataTable<VisitorBookingDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<VisitorBookingDto>();
            }

        }

    }
}
