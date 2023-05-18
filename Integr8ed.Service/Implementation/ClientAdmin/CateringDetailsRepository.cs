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
    public class CateringDetailsRepository : ClientAdminGenericRepository<Catering_Details>, ICateringDetailsServices
    {
        private readonly ClientAdminDbContext _context;
        public CateringDetailsRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<CateringDetailsDto>> GetCateringDetailList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCateringDetailList, paraObjects);
                return Common.ConvertDataTable<CateringDetailsDto>(dataSet.Tables[0]);
            }


        }

        public async Task<CatMenuDto> CatmenuDtoList(string ConnectionString, long id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {       
                var catmenuDto = new CatMenuDto();

                var existingCaterers =  await db.CatererMenu.Where(x => !x.IsDelete).Select(x => x.CatererId ).ToListAsync();
                var catList = await db.Catering_Details.Where(x => !x.IsDelete 
                              && ((id != 0 ? false : true) == true ? true : x.Id == id)
                              //&& id!=0? existingCaterers.Contains(x.Id) : !existingCaterers.Contains(x.Id)
                                ).Select(x =>
                                   new CatererList { CatererId = x.Id, CatererName = x.CatererName }).ToListAsync();
                catmenuDto.CatererList = catList;
                if (id != 0)
                {
                    var menuList = (from cm in db.CatererMenu
                                    join m in db.Menus on cm.MenuId equals m.Id
                                    where cm.CatererId == id && !m.IsDelete && !cm.IsDelete
                                    select new MenuList
                                    {
                                        MenuId = cm.MenuId ?? 0,
                                        MenuName = m.DescriptionOFFood,
                                        Cost=cm.Cost
                                    }

                                  ).ToList();
                    catmenuDto.Menulist = menuList;
                    catmenuDto.AllMenuList = await db.Menus.Where(x => !x.IsDelete).Select(x => new MenuList
                    {
                        MenuId = x.Id,
                        MenuName = x.DescriptionOFFood
                    }).ToListAsync(); 
                }
                else
                {
                    var menuList = await db.Menus.Where(x => !x.IsDelete).Select(x => new MenuList
                    {
                        MenuId = x.Id,
                        MenuName = x.DescriptionOFFood
                    }).ToListAsync();
                    catmenuDto.AllMenuList = menuList;
                }

                return catmenuDto;
            }
        }

        public async Task<List<Catering_Details>> GetCatererList(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = await db.Catering_Details.Where(x => !x.IsDelete).ToListAsync();
                return result;
            }

        }
    }
}
