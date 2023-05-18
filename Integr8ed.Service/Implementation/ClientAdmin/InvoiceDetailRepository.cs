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

namespace Integr8ed.Service.Implementation
{
    public class InvoiceDetailRepository : ClientAdminGenericRepository<InvoiceDetail>, IInvoiceDetailServices
    {
        private readonly ClientAdminDbContext _context;
        public InvoiceDetailRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<InvoiceDetailDto>> GetInvoiceDetailList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetInvoiceDetailList, paraObjects);
                return Common.ConvertDataTable<InvoiceDetailDto>(dataSet.Tables[0]);
            }
        }

        public async Task<InvoiceDetailDto> GET_InvoiceDetailAndItemById(string ConnectionString, long id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var invoiceDetailObj = await (from ind in db.InvoiceDetail
                                       join itemToInv in db.ItemsToInvoice on ind.Id equals itemToInv.InvoiceDetailsId
                                       where ind.Id == id & !ind.IsDelete
                                       select new InvoiceDetailDto
                                       {
                                           Id = ind.Id,
                                           ContactName = ind.ContactName,
                                           InvoiceAddress = ind.InvoiceAddress,
                                           Email = ind.Email,
                                           DateCreated = ind.CreatedDate.ToString(),
                                           InvoiceRequestDate = ind.InvoiceRequestDate,
                                           InvoiceAmount = ind.InvoiceAmount,
                                           VatAmount = ind.VatAmount,
                                           GrossAmount = ind.GrossAmount,
                                           InvoiceMasterId = itemToInv.InvoiceMasterId.Value,
                                           Quantity = itemToInv.Quantity
                                       }).FirstOrDefaultAsync();

                var itemToInvoiceLst = await db.ItemsToInvoice.Where(x => x.InvoiceDetailsId == invoiceDetailObj.Id & !x.IsDelete).ToListAsync();
                List<InvoiceDto> invoiceList = new List<InvoiceDto>();

                foreach(var item in itemToInvoiceLst)
                {
                    var invoice = await db.Invoices.Where(x => x.Id == item.InvoiceMasterId & !x.IsDelete).FirstOrDefaultAsync();
                    var inv = new InvoiceDto()
                    {
                        Title = invoice.Title,
                        Vate = invoice.Vate,
                        IteamCost = invoice.IteamCost,
                        Quantity = item.Quantity
                    };
                    invoiceList.Add(inv);
                }

                invoiceDetailObj.Invoices = invoiceList;
                return invoiceDetailObj;
            }
        }

    }
}
