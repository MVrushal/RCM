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
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class ProfileRepository : ClientAdminGenericRepository<Integr8ed.Data.DbModel.ClientAdmin.Users>, IProfileServices
    {
        private readonly ClientAdminDbContext _context;
        public ProfileRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public List<UserDto> GetUserList(string ConnectionString,long BranchId)
        {
            var userListResult = new List<UserDto>();
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                try
                {
                    var userlist = db.Users.Where(x=>!x.IsDelete && x.BranchId==BranchId).ToList();
                    foreach (var user in userlist)
                    {
                        var result = new UserDto();
                        result.ID = user.Id;
                        result.FirstName = user.FirstName;
                        result.LastName = user.LastName;
                        userListResult.Add(result);
                    }
                }
                catch (Exception e)
                {
                    return null;
                }
            }
            return userListResult;
        }


    }
}
