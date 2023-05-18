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
    public class CatererMenuRepository : ClientAdminGenericRepository<CatererMenu>, ICatererMenuServices
    {
        private readonly ClientAdminDbContext _context;
        public CatererMenuRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<GetCatMenuListDto>> GetCatMenuList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCatMenuList, paraObjects);
                    var catererMenus = Common.ConvertDataTable<GetCatMenuListDto>(dataSet.Tables[0]);
                    return catererMenus;
                }
            }
            catch (Exception ex)
            {
                return new List<GetCatMenuListDto>();
            }

        }


        public async Task<List<GetCatMenuListDto>> GetCatererMenuListByCatererId(string ConnectionString, long catererId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var menuLst = await (from cm in db.CatererMenu
                                         join me in db.Menus on cm.MenuId equals me.Id
                                         where cm.CatererId == catererId
                                         select new GetCatMenuListDto
                                         {
                                             Id = cm.Id,
                                             CatererId = cm.CatererId.Value,
                                             MenuId = me.Id,
                                             Cost = cm.Cost,
                                             DescriptionOFFood = me.DescriptionOFFood
                                         }).ToListAsync();
                    return menuLst;
                }
            }
            catch (Exception ex)
            {
                return new List<GetCatMenuListDto>();
            }

        }

        public List<GetCatMenuListDto> MenuListbyCatererId(string ConnectionString,long CatererId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                var menuList = new List<GetCatMenuListDto>();
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    menuList=(from cm in db.CatererMenu
                              join m in db.Menus on   cm.MenuId equals m.Id
                              join c in db.Catering_Details on cm.CatererId equals c.Id
                              where cm.CatererId== CatererId
                              select new GetCatMenuListDto
                              {
                                  CatererId = cm.CatererId??0,
                                  CatererName = c.CatererName,
                                  Cost = cm.Cost,
                                  MenuId = cm.MenuId??0,
                                  DescriptionOFFood = m.DescriptionOFFood}).ToList();

                    
                }
                return menuList;

            }
            catch (Exception ex)
            {
                return new List<GetCatMenuListDto>();
            }

        }

        public bool  DeleteAllMenu(string ConnectionString, long CatererId)
        {
            try
            {
                
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);

                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var menulist = db.CatererMenu.Where(x => x.CatererId == CatererId).ToList();
                    db.CatererMenu.RemoveRange(menulist);
                    db.SaveChanges();
                    return  true;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

        }

    }
}
