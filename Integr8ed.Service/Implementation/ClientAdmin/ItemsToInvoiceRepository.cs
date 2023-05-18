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
    public class ItemsToInvoiceRepository : ClientAdminGenericRepository<ItemsToInvoice>, IItemsToInvoiceServices
    {
        private readonly ClientAdminDbContext _context;
        public ItemsToInvoiceRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<InvoiceDto>> GetItemsToInvoiceList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetItemsToInvoiceList, paraObjects);
                return Common.ConvertDataTable<InvoiceDto>(dataSet.Tables[0]);
            }
        }

        public async Task<List<ItemsToInvoice>> GET_ItemToInvoiceList(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.ItemsToInvoice.Where(x => !x.IsDelete).ToListAsync();
                return result;
            }
        }

        public async Task<List<ItemsToInvoice>> GetAllItemToInvoiceByInvoiceDetailId(string ConnectionString, long InvoiceDetailId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = await db.ItemsToInvoice.Where(x => x.InvoiceDetailsId == InvoiceDetailId && !x.IsDelete).ToListAsync();
                return result;
            }
        }

        public async Task<InvoiceDetailDto> CalculateInvoiceAmountAndGrossAmount(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {

                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {
                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.CalculateInvoiceAmountAndGrossAmount, paraObjects);
                    var invDet = Common.ConvertDataTable<InvoiceDetailDto>(dataSet.Tables[0]);
                    return invDet.FirstOrDefault();
                }
            }
            catch(Exception ex)
            {
                return new InvoiceDetailDto() { };
            }
        }
    }
}
