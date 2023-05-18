using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface;
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
    public class InvoiceRepository : ClientAdminGenericRepository<Invoice>, IInvoiceServices
    {
        private readonly ClientAdminDbContext _context;
        public InvoiceRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<InvoiceDto> GetInvoiceItemDetailById(string ConnectionString, int Id)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var invoiceDetail = await db.Invoices.FirstOrDefaultAsync(x => x.Id == Id);
                var invoiceDetailDto = new InvoiceDto();
                if (invoiceDetail != null)
                {
                    invoiceDetailDto.Title = invoiceDetail.Title;
                    invoiceDetailDto.Description = invoiceDetail.Description;
                    invoiceDetailDto.Vate = invoiceDetail.Vate;
                    invoiceDetailDto.IteamCost = invoiceDetail.IteamCost;
                    invoiceDetailDto.BudgetRate = invoiceDetail.BudgetRate;
                    invoiceDetailDto.IsIteamVatable = invoiceDetail.IsIteamVatable;
                }
                return invoiceDetailDto;
            }
        }

        public async Task<List<InvoiceDto>> GetInvoiceList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetInvoiceList, paraObjects);
                return Common.ConvertDataTable<InvoiceDto>(dataSet.Tables[0]);
            }

        }

        public async Task<List<Invoice>> GetInvoiceListForDropDown(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = await db.Invoices.Where(x => !x.IsDelete).ToListAsync();
                return result;
            }
        }
    }
}
