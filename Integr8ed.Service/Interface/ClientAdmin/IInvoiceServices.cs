using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.BaseInterface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface.ClientAdmin
{
    public interface IInvoiceServices : IClientAdminGenericService<Invoice>
    {
        Task<List<InvoiceDto>> GetInvoiceList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<Invoice>> GetInvoiceListForDropDown(string ConnectionString);
        Task<InvoiceDto> GetInvoiceItemDetailById(string ConnectionString, int Id);
    }
}
