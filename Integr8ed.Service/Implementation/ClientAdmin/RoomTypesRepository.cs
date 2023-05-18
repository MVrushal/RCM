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
    public class RoomTypesRepository : ClientAdminGenericRepository<Room_Type>, IRoomTypesService
    {
        private readonly ClientAdminDbContext _context;
        public RoomTypesRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public List<Room_Type> GetRoomTypeListForDropDown(string ConnectionString,long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = db.Room_Types.Where(x => !x.IsDelete && x.IsActive && x.BranchId== BranchId).ToList();
                return result;
            }
        }

        public List<Users> GetUserListForDropDown(string ConnectionString, long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = db.Users.Where(x => !x.IsDelete && x.IsActive && x.BranchId == BranchId).ToList();
                return result;
            }
        }

        public async Task<List<RoomTypeListDto>> BindDiary(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            var rmTypeList = new List<RoomTypeListDto>();
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {


                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.BindDiary, paraObjects);

                var roomlist= Common.ConvertDataTable<RoomTypeListDto>(dataSet.Tables[0]);
                //roomlist.ForEach(x => x.ColorCode = getColorCode(x.BookingStatus));
                return roomlist;

            }
        }

        public async Task<List<RoomTypeListDto>> CheckBooking(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            var rmTypeList = new List<RoomTypeListDto>();
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {


                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CheckBooking, paraObjects);

                var roomlist = Common.ConvertDataTable<RoomTypeListDto>(dataSet.Tables[0]);
                //roomlist.ForEach(x => x.ColorCode = getColorCode(x.BookingStatus));
                return roomlist;

            }
        }

        public async Task<List<DayViewReportDTO>> GetWeeklyDiaryReport(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {


                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetWeeklyDiaryReport, paraObjects);

                var roomlist = Common.ConvertDataTable<DayViewReportDTO>(dataSet.Tables[0]);
               
                return roomlist;

            }
        }

        public async Task<List<PDFDto>> GetDailyDiaryReportPDF(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetDayWiseDiaryReportPDF, paraObjects);

                var roomlist = Common.ConvertDataTable<PDFDto>(dataSet.Tables[0]);

                return roomlist;

            }
        } public async Task<List<PDFDto>> GetWeeklyDiaryReportPDF(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetWeeklyDiaryReportPDF, paraObjects);

                var roomlist = Common.ConvertDataTable<PDFDto>(dataSet.Tables[0]);

                return roomlist;

            }
        }

        public async Task<List<PDFDto>> GetMonthlyDiaryListPDF(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {


                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetMonthlyDiaryListPDF, paraObjects);

                var roomlist = Common.ConvertDataTable<PDFDto>(dataSet.Tables[0]);

                return roomlist;

            }
        }

        public async Task<CompanyName> GetOrganizationName(string ConnectionString, SqlParameter[] paraObjects)
        {
            try {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var Orgs = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCompanyName, paraObjects);

                    var OrganizationName = Common.ConvertDataTable<CompanyName>(Orgs.Tables[0]).FirstOrDefault();

                    return OrganizationName;
                }
            } 
            catch (Exception e)
            { 
                return new CompanyName { OrganizationName= "Company Name not found!" }; 
            }
        }

        //public async Task<List<CompanyName>> GetOrganizationName(string ConnectionString, SqlParameter[] paraObjects)
        //{
        //    var dbConnection = new SqlConnection(ConnectionString);
        //    var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
        //    OptionBuilder.UseSqlServer(dbConnection);
        //    using (var db = new ClientAdminDbContext(OptionBuilder.Options))
        //    {
        //        var OrgName = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCompanyName, paraObjects);
        //        //var result = await db.CompanyName.FirstOrDefaultAsync();

        //        var CompanyName = Common.ConvertDataTable<CompanyName>(OrgName.Tables[0]);

        //        return CompanyName;
        //    }
        //}

        public async Task<List<Room_TypeDto>> GetRoomTypesList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetRoomTypesList, paraObjects);
                return Common.ConvertDataTable<Room_TypeDto>(dataSet.Tables[0]);
            }


        }


        public async Task<List<MonthlyViewDto>> GetMonthlyDiaryList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetMonthlyDiaryList, paraObjects);
                return Common.ConvertDataTable<MonthlyViewDto>(dataSet.Tables[0]);
            }


        }


        public List<StandardEquipment> GET_EuipmentList(string ConnectionString,long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = db.StandardEquipment.Where(x => !x.IsDelete && x.BranchId== BranchId).ToList();
                return result;
            }
        }

        public List<Room_Type> GetDiaryRoomList(string ConnetionString, long BranchId)
        {
            var dbConnection = new SqlConnection(ConnetionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = db.Room_Types.Where(x => !x.IsDelete && x.BranchId == BranchId && x.IsActive).ToList();
                return result;
            }
        }


        //public static string getColorCode(string ConnectionString, int statusCode)
        //{
        //    var dbConnection = new SqlConnection(ConnectionString);
        //    var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
        //    OptionBuilder.UseSqlServer(dbConnection);
        //    Booking_Status bs = new Booking_Status();
        //    using (var db = new ClientAdminDbContext(OptionBuilder.Options))
        //    {
        //        var colorCode = db.Booking_Status.Where(x => !x.IsDelete && x.IsActive && x.Id == statusCode).FirstOrDefault();
        //        bs.ColorCode = colorCode.ColorCode;
        //        var ColorCode = bs.ColorCode;
        //        return ColorCode;
        //    }
        //}

        public static string getColorCode(int statusCode)
        {
            string colorCode = "";
            switch (statusCode)
            {
                case 1: colorCode = RoomColors.Confirmed; break;
                case 2: colorCode = RoomColors.Canceled; break;
                case 3: colorCode = RoomColors.Pending; break;
                case 4: colorCode = RoomColors.Reserved; break;
                case 5: colorCode = RoomColors.Deposit; break;
                case 6: colorCode = RoomColors.Paid; break;
                case 7: colorCode = RoomColors.Waiting; break;
                case 8: colorCode = RoomColors.PreBooking; break;
            }

            return colorCode;
        }

        public bool InsertImage(string ConnectinString, List<RoomTypeImageDto> model)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectinString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    foreach (var item in model)
                    {
                        var obj = new RoomImageMaster();
                        obj.RoomId = item.RoomId;   
                        obj.RoomImage = item.ImageName;
                        db.RoomImageMaster.Add(obj);
                        db.SaveChanges();
                    }
                    return true;
                }
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public RoomTypeImageDto GetImages(string ConnectionString, long Id)
        {
           
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            var roomimg = new RoomTypeImageDto();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var objlist=db.RoomImageMaster.Where(x => x.RoomId == Id).ToList();
                roomimg.roomImagesList = new List<string>();
                if (objlist.Count() == 0)
                {
                    roomimg.RoomId = Id;
                }
                foreach (var item in objlist)
                {
                    roomimg.RoomId = item.RoomId;
                    roomimg.roomImagesList.Add(item.RoomImage);
                }

                return roomimg;
            }
        }

        public bool DeleteImage(string Connectionstring, long Id, string ImageName)
        {

            try
            {
                var dbConnection = new SqlConnection(Connectionstring);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var obj = db.RoomImageMaster.FirstOrDefault(x => x.RoomImage == ImageName && x.RoomId == Id);
                    db.RoomImageMaster.Remove(obj);
                    var rv = db.SaveChanges();

                }
                return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }
    }
}
