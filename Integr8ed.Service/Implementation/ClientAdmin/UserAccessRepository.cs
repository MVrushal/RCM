using Integr8ed.Data;
using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Enums;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface.ClientAdmin;

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class UserAccessRepository : ClientAdminGenericRepository<Integr8ed.Data.DbModel.ClientAdmin.UserAccess>, IUserAccessService
    {
        private readonly ClientAdminDbContext _context;
        public UserAccessRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public void  DeleteAllAccess(string ConnectionString, long id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var userlist = db.UserAccess.Where(x => x.UserId == id)
                            .Select(x => new UserAccess
                            {
                                Id = Convert.ToInt64(x.Id.ToString()),
                                UserId = Convert.ToInt64(x.UserId.ToString()),
                                MenuId= Convert.ToInt64(x.MenuId.ToString())

                            }).ToList();
                db.RemoveRange(userlist);
                db.SaveChanges();

                //var Parameters = new List<SqlParameter>
                //{
                //    new SqlParameter("@Id",SqlDbType.BigInt){Value = id},

                //};
                //var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.deleteAccess, Parameters.ToArray());
                
            }
        }

        public UserAccessDto GetUserList(string ConnectionString,long id)
        {
               
            var result = new UserAccessDto();

            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                try
                {

                    // var dbSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.UserAccess);
                    // var userlist = Common.ConvertDataTable<UserAccessList>(dbSet.Tables[0]);
                    var userResult = db.Users.Where(x => x.Id == id).FirstOrDefault();
                    var roleResult = db.UserRoles.FirstOrDefault(x=>x.UserId==userResult.Id);
                    if (roleResult != null && (roleResult.RoleId == 1 || roleResult.RoleId == 2))
                    {
                       
                        result.RoomType = true;
                        result.EquipmentRequirment = true;
                        result.DelegetsCodes = true;
                        result.MeetingType = true;
                        result.InvoiceItem = true;
                        result.EntryType = true;
                        result.UserGroup = true;
                        result.CateringDetail = true;
                        result.RoomAvailability = true;
                        result.Internal_ExternalBooking = true;
                        result.RecurringBooking = true;
                        result.BookingDiary = true;
                        result.Reports = true;
                        result.ManageBranch = true;
                        result.UserId = userResult.Id;
                        result.BranchId = userResult.BranchId??0;
                        result.BranchAdmin = roleResult.RoleId == 2 ? true : false;
                        result.IsAdmin = roleResult.RoleId == 1 ? true : false;
                        result.BranchListID = userResult.AdminBranchId;
                    }
                    else {
                    result.IsAdmin = userResult.IsAdmin ?? false;
                     result.BranchId = userResult.BranchId ?? 0;
                        var userlist = db.UserAccess.Where(x=>x.UserId==id).Select(x => new {Id=Convert.ToInt64(x.Id.ToString()), UserId = Convert.ToInt64(x.UserId.ToString()), MenuId = Convert.ToInt64(x.MenuId.ToString()) }).ToList();
                    foreach (var user in userlist)
                    {
                        if (user.UserId==id)
                        {
                            result.UserId =user.UserId;
                                result.BranchListID = userResult.AdminBranchId;
                                switch (user.MenuId)
                            {
                                case (int)UserMenu.Access.RoomType: result.RoomType = true; break;
                                case (int)UserMenu.Access.EquipmentRequirement: result.EquipmentRequirment = true; break;
                                case (int)UserMenu.Access.DelegetsCodes: result.DelegetsCodes = true; break;
                                case (int)UserMenu.Access.MeetingType: result.MeetingType = true; break;
                                case (int)UserMenu.Access.InvoiceItem: result.InvoiceItem = true; break;
                                case (int)UserMenu.Access.EntryType: result.EntryType = true; break;
                                case (int)UserMenu.Access.UserGroup: result.UserGroup = true; break;
                                case (int)UserMenu.Access.CateringDetail: result.CateringDetail = true; break;
                                case (int)UserMenu.Access.RoomAvailability: result.RoomAvailability = true; break;
                                case (int)UserMenu.Access.Internal_ExternalBooking: result.Internal_ExternalBooking = true; break;
                                case (int)UserMenu.Access.RecurringBooking: result.RecurringBooking = true; break;
                                case (int)UserMenu.Access.BookingDiary: result.BookingDiary = true; break;
                                case (int)UserMenu.Access.Reports: result.Reports = true; break;
                                case (int)UserMenu.Access.ManageBranch: result.ManageBranch = true; break;
                            }
                        }

                    }
                    }
                }
                catch (Exception e)
                {

                    return null; 
                }
            
            }
            return result;
        }
       

    }
}
