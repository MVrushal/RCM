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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class BookingDetailRepositroy : ClientAdminGenericRepository<BookingDetails>, IBookingDetailServices
    {

        private readonly ClientAdminDbContext _context;

        public BookingDetailRepositroy(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookingDetailsForGridDto>> GetBookingDetailList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetBookingDetailList, paraObjects);
                    var test = Common.ConvertDataTable<BookingDetailsForGridDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<BookingDetailsForGridDto>();
            }

        }

        public List<BookingDetails> GetBookingDetailForDelete(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = db.BookingDetails.Where(x=> x.IsActive == true && x.IsDelete == false).ToList();
                return result;
            }
        }

        public async Task<List<RoomAvailiblityDto>> GetRoomAvailiblity(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetRoomAvailiblity, paraObjects);
                    var test = Common.ConvertDataTable<RoomAvailiblityDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<RoomAvailiblityDto>();
            }

        }

        public async Task<List<RoomAvailiblityDto>> GetRoomAvailiblityforDownloadExcel(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetRoomAvailiblityDownloadExcel, paraObjects);
                    var test = Common.ConvertDataTable<RoomAvailiblityDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<RoomAvailiblityDto>();
            }

        }

        public async Task<decimal> CalculateBookingCostByDate(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CalculateBookingCostByDate, paraObjects);
                    var test = Convert.ToInt64(dataSet.Tables[0].Rows[0].ItemArray[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public async Task<bool> CheckRecurringbookingAvailable(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CheckRecurringbookingAvailable, paraObjects);
                    var test = Common.ConvertDataTable<RecurringBookingsDto>(dataSet.Tables[0]);
                    if (test.Count() > 0)
                        return false;
                    else 
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public async Task<List<BookingDetailsDto>> CheckIsBookingDetailAvailable(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CheckIsBookingDetailAvailable, paraObjects);
                    var test = Common.ConvertDataTable<BookingDetailsDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<BookingDetailsDto>();
            }

        }

        public async Task<BookingDetails> CheckBookingDetailMeetingTitle(string ConnectionString, string TitleOfMeeting,long BranchId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var booking = await db.BookingDetails.FirstOrDefaultAsync(x => x.TitleOfMeeting.ToLower().Trim() == TitleOfMeeting.ToLower().Trim() && x.IsDelete == false && x.BranchId== BranchId);
                    return booking;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public List<VisitorBookingDto> GetVisitorList(string ConnectionString, long BookingId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var getVisitorId = db.VisitorBooking.Where(x => x.BookingDetailId == BookingId).Select(x=>x.VisitorId);

                    var vlist = db.Visitors.Where(x => getVisitorId.Contains(x.Id)).Select(x => new VisitorBookingDto {
                    Mobile=x.Mobile,
                    Name=x.Name,
                    SurName=x.SurName,
                    PostCode=x.PostCode,
                    Email=x.Email,
                    Telephone=x.Telephone,
                    Notes=x.Notes,
                    Address=x.Address
                    
                    }).ToList();



                    return vlist;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<AvalDto>> GetbookingTableList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetbookingTableList, paraObjects);
                    var test = Common.ConvertDataTable<AvalDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<AvalDto>();
            }

        }

        public int GetBookingDetails(string ConnectionString, BookingDetailsDto bookingDetail)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var getbookings = db.BookingDetails.Where(x => x.RoomTypeId == bookingDetail.RoomTypeId
                                            && x.StartTime == bookingDetail.StartTime
                                            && x.FinishTime == bookingDetail.FinishTime
                                            && x.BookingDate == bookingDetail.BookingDate
                                            && x.IsDelete == false
                                            && (((DateTime.ParseExact(x.StartTime, "HH:mm", CultureInfo.InvariantCulture) > DateTime.ParseExact(bookingDetail.StartTime, "HH:mm", CultureInfo.InvariantCulture))
                                            && (DateTime.ParseExact(x.FinishTime, "HH:mm", CultureInfo.InvariantCulture) < DateTime.ParseExact(bookingDetail.FinishTime, "HH:mm", CultureInfo.InvariantCulture)))
                                            ));

                    return getbookings.Count();
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public int IsExistBooking(string ConnectionString, BookingDetailsDto bookingDetail) {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var getbookings = db.BookingDetails.Where(x => x.RoomTypeId == bookingDetail.RoomTypeId
                                            && x.BookingDate.Day == DateTime.Parse(bookingDetail.BookingDateForDisplay).Day
                                            && x.BookingDate.Month == DateTime.Parse(bookingDetail.BookingDateForDisplay).Month
                                            && x.BookingDate.Year == DateTime.Parse(bookingDetail.BookingDateForDisplay).Year
                                            && !x.IsDelete).ToList();
                                            //&&  TimeSpan.Parse(x.StartTime) < TimeSpan.Parse(bookingDetail.FinishTime)
                                            //&& TimeSpan.Parse(bookingDetail.StartTime) < TimeSpan.Parse(x.FinishTime)
                                            //).ToList();
                    
                   var ExistingCounts=getbookings.Where(x => TimeSpan.Parse(x.StartTime) < TimeSpan.Parse(bookingDetail.FinishTime) && 
                            TimeSpan.Parse(bookingDetail.StartTime) < TimeSpan.Parse(x.FinishTime)).Count();
                    return ExistingCounts;
                }
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

    }
}
