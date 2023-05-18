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

namespace Integr8ed.Service.Interface.ClientAdmin
{
    public class ClientAdminDashboardRepository : IClientAdminDashboardServices
    {


        public async Task<DashboardDto> GetClientAdminDashboardData(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.DashboardDetailList, paraObjects);
                    var DashboardDto = new DashboardDto
                    {
                        bookingDetailExternalcountWises = Common.ConvertDataTable<BookingDetailExternalcountWise>(dataSet.Tables[0]),
                        AdminDasboardCount = Common.ConvertDataTable<DasboardCount>(dataSet.Tables[1]),
                        bookingDetailCountStatusWises = Common.ConvertDataTable<BookingDetailCountStatusWises>(dataSet.Tables[2]),
                        bookingDetailCountMeetingWises = Common.ConvertDataTable<bookingDetailCountMeetingWises>(dataSet.Tables[3]),
                        bookingDetailCountMonthWises = Common.ConvertDataTable<BookingDetailCountMonthWise>(dataSet.Tables[4]),
                        bookingDetailCountRoomWiseMultis = Common.ConvertDataTable<BookingDetailCountRoomWiseMulti>(dataSet.Tables[5]),

                    };
                    return DashboardDto;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }
      
        public async Task<List<BookingDetailsForGridDto>> GetBookingCountDetail(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetBookingChartDatadetail, paraObjects);

                    var test = Common.ConvertDataTable<BookingDetailsForGridDto>(dataSet.Tables[0]);
                    return test;
                }
                catch(Exception ex)
                {
                   return  new List<BookingDetailsForGridDto>();
                }
               
            }

        }



        public async Task<List<ToolTip>> GetBookingCountToolTipdetail(string ConnectionString, string Roomname,int BookingType)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                if (BookingType == 1)
                {
                    var Data = await (from eq in db.BookingDetails
                                      join seq in db.Room_Types on eq.RoomTypeId equals seq.Id
                                      where !eq.IsDelete && !seq.IsDelete && seq.Title == Roomname && eq.ExternalBookingClientId==null
                                      select new ToolTip
                                      {
                                          BookingDate = eq.BookingDate + " ( " + eq.StartTime + " - " + eq.FinishTime + " )",
                                          EndDate = eq.FinishTime,
                                          StartTime = eq.StartTime
                                      }).ToListAsync();

                    return Data;
                }
                else {
                    var Data = await (from eq in db.BookingDetails
                                      join seq in db.Room_Types on eq.RoomTypeId equals seq.Id
                                      where !eq.IsDelete && !seq.IsDelete && seq.Title == Roomname && eq.ExternalBookingClientId != null
                                      select new ToolTip
                                      {
                                          BookingDate = eq.BookingDate + " ( " + eq.StartTime + " - " + eq.FinishTime + " )",
                                          EndDate = eq.FinishTime,
                                          StartTime = eq.StartTime
                                      }).ToListAsync();

                    return Data;

                }
               
            }

        }
        public async Task<List<ToolTip>> GetMeetingCountToolTipdetail(string ConnectionString, string Roomname)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var Data = await (from eq in db.BookingDetails
                                  join seq in db.MeetingTypes on eq.MeetingTypeId equals seq.Id
                                  where !eq.IsDelete && !seq.IsDelete && seq.Title == Roomname
                                  select new ToolTip
                                  {
                                      BookingDate = eq.BookingDate + " ( " + eq.StartTime + " - " + eq.FinishTime + " )",
                                    
                                  }).ToListAsync();

                return Data;
            }

        }
    }


}
    

