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
    public class MenuRepository : ClientAdminGenericRepository<Menu>, IMenuServices
    {
        private readonly ClientAdminDbContext _context;
        public MenuRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<MenuDto>> GetMenuList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetMenuList, paraObjects);
                return Common.ConvertDataTable<MenuDto>(dataSet.Tables[0]);
            }
        }

        public async Task<List<Menu>> GetMenuListForDropDown(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.Menus.Where(x => !x.IsDelete).ToListAsync();
                return result;
            }
        }

        public async Task<GetCatMenuListDto> GetMenuDetailById(string ConnectionString, long Id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var menuDto = await (from cm in db.CatererMenu
                                     join me in db.Menus on cm.MenuId equals me.Id
                                     where cm.Id == Id
                                     select new GetCatMenuListDto
                                     {
                                         Id = cm.Id,
                                         CatererId = cm.CatererId.Value,
                                         MenuId = me.Id,
                                         Cost = cm.Cost,
                                         DescriptionOFFood = me.DescriptionOFFood
                                     }).FirstOrDefaultAsync();

                return menuDto;
            }
        }

    }
}
