using Integr8ed.Data;
using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.BaseService;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation
{

    public class UserRepository : ClientAdminGenericRepository<Users>, IUserService
    {
        private readonly ClientAdminDbContext _context;
        public UserRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public bool AddExternalUser(string ConnectionString, ExternalUserDto model)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var user = new Users();
                    user.Email = model.Email;
                    user.Password = model.Password;
                    user.FirstName = model.FirstName;
                    user.LastName = model.LastName;
                    user.MobileNumber = model.ContactNumber;

                    db.Users.Add(user);
                    var addUserResult = db.SaveChanges();
                    if (addUserResult >= 1)
                    {
                        var externalUserRole = new Integr8ed.Data.DbModel.ClientAdmin.UserRoles();
                        externalUserRole.UserId = user.Id;
                        externalUserRole.RoleId = 4;
                        db.UserRoles.Add(externalUserRole);
                        var roleResult = db.SaveChanges();
                        if (roleResult >= 1)
                            return true;
                        else
                            return false;

                    }
                    else
                        return false;
                }
            }
            catch (Exception e)
            {

                return false;
            }
        }

        public void AddRoles(string ConnectionString, long AdminId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                db.UserRoles.Add(new Integr8ed.Data.DbModel.ClientAdmin.UserRoles { UserId = AdminId, RoleId = 1 });
                db.SaveChanges();
            }
        }

        public async Task<List<Users>> GetUsersList(string ConnectionString, long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                if (BranchId == 0)
                {
                    var result = await db.Users.Where(x => !x.IsDelete && x.IsActive).ToListAsync();
                    return result;
                }
                else
                {
                    var result = await db.Users.Where(x => !x.IsDelete && x.IsActive && x.BranchId == BranchId).ToListAsync();
                    return result;
                }
            }
        }

        public async Task<List<Users>> GetUserName(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                    var result = await db.Users.Where(x => !x.IsDelete).ToListAsync();
                    return result;
            }
        }

        public Users GetCompanyAdmin(string ConnectionString, long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var admin = (from user in db.Users
                             join uroles in db.UserRoles on user.Id equals uroles.UserId
                             where uroles.RoleId == (int)UserRolesEnum.ClientAdmin && user.BranchId==BranchId
                             select user
                           ).FirstOrDefault();
              
                return admin;
            }
        }
    }
    public class SuparAdminRepository : GenericRepository<ApplicationUser>, ISuparAdminUserService
    {
        private readonly ApplicationDbContext _context;
        public SuparAdminRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }


    }
}



