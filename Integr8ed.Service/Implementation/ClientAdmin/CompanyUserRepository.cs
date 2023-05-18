using Integr8ed.Data;
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
    public class CompanyUserRepository : ClientAdminGenericRepository<Integr8ed.Data.DbModel.ClientAdmin.Users>, ICompanyUserService
    {
        private readonly ClientAdminDbContext _context;
        public CompanyUserRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<UserDto>> GetCompanyUserList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCompanyUserList, paraObjects);
                return Common.ConvertDataTable<UserDto>(dataSet.Tables[0]);
            }


        }

        public async Task MakeBranchAdmin(string ConnectionString,long UserId,bool isBranchAdmin,string BranchlistId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var isExist = await db.UserRoles.FirstOrDefaultAsync(x => x.UserId == UserId);

                if (isExist == null)
                {
                    isExist = new Integr8ed.Data.DbModel.ClientAdmin.UserRoles();
                    isExist.RoleId = isBranchAdmin?2:3;//
                    isExist.UserId = UserId;
                    db.UserRoles.Add(isExist);
                   db.SaveChanges();

                    var user = await db.Users.FirstOrDefaultAsync(x => x.Id==UserId);
                    user.AdminBranchId = BranchlistId;
                    db.Users.Update(user);
                    db.SaveChanges();
                }
                else {


                    if (isBranchAdmin)
                    {   
                        isExist.RoleId = 2;//Making Branch admin by adding BranchAdmin role id
                        db.UserRoles.Update(isExist);
                        db.SaveChanges();
                        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == UserId);
                        user.AdminBranchId = BranchlistId;
                        db.Users.Update(user);
                        db.SaveChanges();
                    }
                    else
                    {
                        isExist.RoleId = 3;
                        db.UserRoles.Update(isExist);
                        db.SaveChanges();
                        var user = await db.Users.FirstOrDefaultAsync(x => x.Id == UserId);
                        user.AdminBranchId = BranchlistId;//Allow all the normal users to have access the branches assigned by company/branch admin - Aman
                        db.Users.Update(user);
                        db.SaveChanges();
                    }

                    
                }
                
            }


        }

    }
}
