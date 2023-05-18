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
    public class Cat_Req_MenuRepository : ClientAdminGenericRepository<Cat_Req_Menu>, ICat_Req_MenuServices
    {
        private readonly ClientAdminDbContext _context;
        public Cat_Req_MenuRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Cat_Req_MenuDto>> GetCat_Req_MenuList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCat_Req_MenuList, paraObjects);
                    var test = Common.ConvertDataTable<Cat_Req_MenuDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<Cat_Req_MenuDto>();
            }

        }

        public async Task<List<Cat_Req_Menu>> GetCat_Req_MenuListByCatReqId(string ConnectionString, long cat_ReqId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var catReq = await db.Cat_Req_Menu.Where(x => x.Cat_ReqId == cat_ReqId && !x.IsDelete).ToListAsync();
                    return catReq;
                }
            }
            catch (Exception ex)
            {
                return new List<Cat_Req_Menu>();
            }
        }
    }
}
