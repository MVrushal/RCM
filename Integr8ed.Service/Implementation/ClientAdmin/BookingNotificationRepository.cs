
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
    public class BookingNotificationRepository : ClientAdminGenericRepository<BookingDetails>, IBookingNotificationServices
    {
        private readonly ClientAdminDbContext _context;
        public BookingNotificationRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<bool> DeleteBookingByCompanyCodeANDId(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.DeleteBookingbyIdANDComCode, paraObjects);
                    var result = Convert.ToBoolean(dataSet.Tables.Count);
                    return result;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<List<BookingDetailsDto>> GetBookingList(string ConnectionString)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    DateTime NotifyDate = DateTime.Now;
                    var booking = db.BookingDetails.Where(x => x.IsDelete == false && x.BookingDate.Date >= NotifyDate).Select(x => new BookingDetailsDto { 
                        Id = x.Id,
                        TitleOfMeeting = x.TitleOfMeeting,
                        BookingDate = x.BookingDate,
                        Cost = x.Cost,
                        NotifyDays = x.NotifyDays
                    }).ToList();
                    return booking;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
