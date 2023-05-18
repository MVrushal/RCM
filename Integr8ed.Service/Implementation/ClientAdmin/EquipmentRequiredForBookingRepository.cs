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
    public class EquipmentRequiredForBookingRepository : ClientAdminGenericRepository<EquipmentRequiredForBooking>, IEquipmentRequiredForBookingServices
    {
        private readonly ClientAdminDbContext _context;
        public EquipmentRequiredForBookingRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }


        public async Task<List<CheckEquipmetIsavailable>> CheckBookingStatusEquipmentAvalable(string ConnectionString, SqlParameter[] sp)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.BookingStatusCheckIsEquipmentAvalable, sp);

                    return Common.ConvertDataTable<CheckEquipmetIsavailable>(dataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {
                return new List<CheckEquipmetIsavailable>();
            }
        }

        public async Task<decimal> CheckISEquipmentAvalable(string ConnectionString, SqlParameter[] sp)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CheckISEquipmentAvalable, sp);

                    return Convert.ToInt64(dataSet.Tables[0].Rows[0].ItemArray[0]);
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task<List<EquipmentRequiredForBookingDto>> GetEquipmentRequiredForBookingList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetEquipmentRequiredForBookingList, paraObjects);
                    return Common.ConvertDataTable<EquipmentRequiredForBookingDto>(dataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {
                return new List<EquipmentRequiredForBookingDto>();
            }

        }

        public async Task<List<EquipmentRequiredForBooking>> GET_EuipmentReqForBookingList(string ConnectionString, long bookingDetailId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.EquipmentRequiredForBooking.Where(x => !x.IsDelete && x.BookingDetailId == bookingDetailId).ToListAsync();
                return result;
            }
        }

    }
}
