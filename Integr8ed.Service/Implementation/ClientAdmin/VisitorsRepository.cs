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
    public class VisitorsRepository : ClientAdminGenericRepository<Visitors>, IVisitorsService
    {
        private readonly ClientAdminDbContext _context;
        public VisitorsRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<VisitorDto>> GetVisitorList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetVisitorList, paraObjects);
                return Common.ConvertDataTable<VisitorDto>(dataSet.Tables[0]);
            }
        }
        public async Task<List<VisitorDto>> loadUserMobile(string ConnectionString, string mobile)
        {
            var result = new List<VisitorDto>();
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var visitorList = await (from a in db.Visitors where a.Mobile.StartsWith(mobile) select new VisitorDto
                {
                    Id = a.Id,
                    Mobile = a.Mobile,
                    Name = a.Name,
                    SurName = a.SurName,
                    Address = a.Address,
                    Email = a.Email,
                    Description=a.Description,
                    Telephone=a.Telephone,
                    Notes=a.Notes,
                    PostCode=a.PostCode

                }).ToListAsync();
                result.AddRange(visitorList);
            }
            return result;
        }
    }
}
