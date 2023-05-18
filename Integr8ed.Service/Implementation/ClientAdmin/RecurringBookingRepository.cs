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
    public class RecurringBookingRepository : ClientAdminGenericRepository<RecurringBookings>, IRecurringBookingServices
    {
        private readonly ClientAdminDbContext _context;
        public RecurringBookingRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookingDetailsForGridDto>> GetRecurringBookingDetailList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetRecurringBookingDetailList, paraObjects);
                    var result = Common.ConvertDataTable<BookingDetailsForGridDto>(dataSet.Tables[0]);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return new List<BookingDetailsForGridDto>();
            }

        }

        public string CheckIsBookingAvailable(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = db.GetQueryDatatable(SpConstants_ClientAdmin.CheckIsBookingAvailableForRecurringBooking, paraObjects);
                    var result = dataSet.Tables[0].Rows[0].ItemArray[0].ToString();
                    return result;
                }
            }
            catch (Exception ex)
            {
                return "";
            }
        }

        public async Task<List<RecurringBookingsList>> GetRecurringBookingReportPDF(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CheckRecurringbookingAvailableList, paraObjects);

                var recurringBookingsLists = Common.ConvertDataTable<RecurringBookingsList>(dataSet.Tables[0]);

                return recurringBookingsLists;

            }
        }
    }
}
