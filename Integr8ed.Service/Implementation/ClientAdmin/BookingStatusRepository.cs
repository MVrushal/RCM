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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class BookingStatusRepository : ClientAdminGenericRepository<BookingStatus>, IBookingStatusService
    {
        private readonly ClientAdminDbContext _context;
        public BookingStatusRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<BookingStatusDto>> GetBookingStatusList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetBookingStatusList, paraObjects);
                return Common.ConvertDataTable<BookingStatusDto>(dataSet.Tables[0]);
            }
        }

        public List<BookingStatus> GetBookingStatusListForDropDown(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = db.BookingStatus.Where(x=>x.IsActive == true && x.IsDelete == false).ToList();
                return result;
            }
        }
    }
}
