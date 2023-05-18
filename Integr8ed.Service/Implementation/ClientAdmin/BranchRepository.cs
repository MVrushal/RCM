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
using static Integr8ed.Service.Enums.GlobalEnums;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class BranchRepository : ClientAdminGenericRepository<BranchMaster>, IBranchService
    {
        private readonly ClientAdminDbContext _context;
        public BranchRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }
        public List<BranchMaster> getBranchList(string ConnectionString, string userID)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    if (userID == "")
                    {
                        var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true).ToList();
                        return dataSet;
                    }
                    var userResult = db.Users.Where(x => x.Id == Convert.ToInt64(userID)).FirstOrDefault();
                    if (userResult != null)
                    {
                        if (userResult.IsAdmin == true)
                        {
                            var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true).ToList();
                            return dataSet;
                        }
                        else
                        {

                            //Allow all the normal users to acess the branches assigned by company/branch admin  -AMAN 
                            var adminbranchList = userResult.AdminBranchId.Split(",").ToList();

                            var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true && adminbranchList.Contains(x.Id.ToString())).ToList();
                            return dataSet;
                            //var userroles = db.UserRoles.FirstOrDefault(x => x.UserId == Convert.ToInt64(userID));
                            //if ((int)userroles.RoleId == (int)UserRolesEnum.BranchAdmin)
                            //{
                            //    var adminbranchList = userResult.AdminBranchId.Split(",").ToList();

                            //    var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true && adminbranchList.Contains(x.Id.ToString())).ToList();
                            //    return dataSet;
                            //}
                            //else {
                            //    var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true).ToList();
                            //    return dataSet;
                            //}
                        }
                    }
                    else {
                        var dataSet = db.BranchMaster.Where(x => x.IsDelete == false && x.IsActive == true).ToList();
                      
                        return dataSet;
                    }


                    
                }
            }
            catch (Exception ex)
            {
                return new List<BranchMaster>();
            }

        }


        public int GetBookingRequestCounts(string ConnectionString, string userID,long BranchId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var requestCount = (from b in db.BookingDetails
                                        join u in db.Users on b.ExternalBookingClientId equals u.Id
                                        join r in db.UserRoles on u.Id equals r.UserId
                                        where r.RoleId == 4 && b.IsActive && b.BranchId==BranchId && b.BookingStatus==3
                                        select new { b.Id }
                                          ).ToList().Count;
                    return requestCount;
                }
                    


                
            }
            catch (Exception ex)
            {
                return 0;
            }

        }

        public List<Room_TypeDto> GetRoomList(string ConnectionString,long Id)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    
                    var dataSet = db.Room_Types.Where(x => x.IsDelete == false && x.IsActive == true && x.BranchId==Id)
                        .Select(x=>new Room_TypeDto { 
                         Id=x.Id,
                         RoomTitle=x.Title,
                         Description=x.Description,
                         HourlyRate=x.HourlyRate,
                         Maxperson=x.Maxperson,
                         Title=x.Title,
                         Floor=x.Floor,
                         IsActive=x.IsActive,
                         Notes=x.Notes,
                         SaturdayRate=x.SaturdayRate,
                         SundayRate=x.SundayRate,
                         roomImageList=db.RoomImageMaster.Where(y=>y.RoomId==x.Id).Select(y=>y.RoomImage).ToArray()
                        })
                        .ToList();
                    return dataSet;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }

       

        public async Task<List<BranchDto>> BranchMasterList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetBranchList, paraObjects);
                return Common.ConvertDataTable<BranchDto>(dataSet.Tables[0]);
            }


        }

    }
}
