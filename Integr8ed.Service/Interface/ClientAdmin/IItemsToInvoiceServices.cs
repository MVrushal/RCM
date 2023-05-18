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
    public interface IItemsToInvoiceServices : IClientAdminGenericService<ItemsToInvoice>
    {
        Task<List<InvoiceDto>> GetItemsToInvoiceList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<ItemsToInvoice>> GET_ItemToInvoiceList(string ConnectionString);
        Task<List<ItemsToInvoice>> GetAllItemToInvoiceByInvoiceDetailId(string ConnectionString, long InvoiceDetailId);
        Task<InvoiceDetailDto> CalculateInvoiceAmountAndGrossAmount(string ConnectionString, SqlParameter[] paraObjects);
    }
}
