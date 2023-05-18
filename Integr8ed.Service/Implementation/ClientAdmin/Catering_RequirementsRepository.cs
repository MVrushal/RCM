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
    public class Catering_RequirementsRepository : ClientAdminGenericRepository<Catering_Requirements>, ICatering_RequirementsServices
    {
        private readonly ClientAdminDbContext _context;
        public Catering_RequirementsRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Catering_RequirementsDto>> GetCatering_RequirementsList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetCateringRequirementList, paraObjects);
                    var test = Common.ConvertDataTable<Catering_RequirementsDto>(dataSet.Tables[0]);
                    return test;
                }
            }
            catch (Exception ex)
            {
                return new List<Catering_RequirementsDto>();
            }

        }
        public List<GetCatMenuListDto> GetCateringReqMenu(string ConnectionString, long CatererId)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                var menuList = new List<GetCatMenuListDto>();
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    menuList = (from cm in db.CatererMenu
                                join m in db.Menus on cm.MenuId equals m.Id
                                join c in db.Cat_Req_Menu on cm.Id equals c.CatererMenuId
                                where c.Cat_ReqId == CatererId && !c.IsDelete
                                select new GetCatMenuListDto
                                {
                                    CatererId = cm.CatererId ?? 0,

                                    Cost = cm.Cost,
                                    MenuId = cm.MenuId ?? 0,
                                    DescriptionOFFood = m.DescriptionOFFood
                                }).ToList();


                }
                return menuList;

            }
            catch (Exception ex)
            {
                return new List<GetCatMenuListDto>();
            }

        }
        public async Task<List<Catering_Requirements>> GET_CateReqList(string ConnectionString, long bookingDetailId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.Catering_Requirements.Where(x => !x.IsDelete && x.BookingDetailId == bookingDetailId).ToListAsync();
                return result;
            }
        }

        public async Task<Catering_RequirementsDto> GET_CateReqAndMenuItemById(string ConnectionString, long id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var catReqObj = await (from cr in db.Catering_Requirements
                                       where cr.Id == id && !cr.IsDelete
                                       select new Catering_RequirementsDto
                                       {
                                           Id = cr.Id,
                                           CatererId = cr.CatererId,
                                           Notes = cr.Notes,
                                           TimeFor = cr.TimeFor,
                                           TimeCollected = cr.TimeCollected,
                                           NumberOfPeople = cr.NumberOfPeople,
                                           Cost = cr.Cost,
                                           BookingDetailId = cr.BookingDetailId,
                                           Cat_Req_Menu = (from crme in db.Cat_Req_Menu
                                                           where crme.Cat_ReqId == id && !crme.IsDelete
                                                           select new Cat_Req_MenuDto
                                                           {
                                                               CatererMenuId = crme.CatererMenuId,
                                                               Cat_ReqId = crme.Cat_ReqId
                                                           }).ToList(),

                                           MenuItemAndCosts = (from crme in db.Cat_Req_Menu
                                                               join crm in db.CatererMenu on crme.CatererMenuId equals crm.Id
                                                               join menu in db.Menus on crm.MenuId equals menu.Id
                                                               where crme.Cat_ReqId == id && !crme.IsDelete && crm.Id == crme.CatererMenuId && !crm.IsDelete && !menu.IsDelete
                                                               select new MenuItemAndCost
                                                               {
                                                                   Menu = " <span class='select-pure__selected-label'> " + menu.DescriptionOFFood + "(" + crm.Cost + ") <i class='fa fa-times' data-value='28'></i> </span>"
                                                               }).ToList()
                                       }).FirstOrDefaultAsync();

                //catReqObj.Cat_Req_Menu = new List<Cat_Req_MenuDto>();

                //var catReqMenuList = await db.Cat_Req_Menu.Where(x => x.Cat_ReqId == id).ToListAsync();

                //catReqObj.Cat_Req_Menu = catReqMenuList.Select(s => new Cat_Req_MenuDto
                //{
                //    CatererMenuId = s.CatererMenuId,
                //    Cat_ReqId = s.Cat_ReqId
                //}).ToList();
                return catReqObj;
            }
        }

        public async Task<List<CatererRequirementInvoiceDto>> CateringRequirementViewAsPDF(string ConnectionString, long bookingDetailId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            List<CatererRequirementInvoiceDto> modelList = new List<CatererRequirementInvoiceDto>();
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.Catering_Requirements.Where(x => !x.IsDelete && x.BookingDetailId == bookingDetailId).ToListAsync();
                foreach (var item in result)
                { 


                    var catreQlist = await db.Cat_Req_Menu.Where(x => x.Cat_ReqId == item.Id).ToListAsync();
                    var MenuList = await db.CatererMenu.Where(y => catreQlist.Select(x => x.CatererMenuId).ToList().Contains(y.Id)).ToListAsync();
                    var bdObj = db.BookingDetails.FirstOrDefault(x => x.Id == item.BookingDetailId);
                    foreach (var menuItem in MenuList)
                    { 
                    CatererRequirementInvoiceDto model = new CatererRequirementInvoiceDto();
                   
                    model.BookingDetailId = item.BookingDetailId??0;
                    model.CatererName = db.Catering_Details.FirstOrDefault(x => x.Id == item.CatererId).CatererName;
                    model.NoOfPeople = item.NumberOfPeople;
                    model.Menu = db.Menus.FirstOrDefault(x => x.Id == menuItem.MenuId).DescriptionOFFood;
                    model.PricePerItem = menuItem.Cost;
                    model.BookingDate = bdObj.BookingDate.ToShortDateString();
                    model.BookingContact =bdObj.BookingContact;
                    modelList.Add(model);
                }

                }

                return modelList;
            }
        }


    }
}
