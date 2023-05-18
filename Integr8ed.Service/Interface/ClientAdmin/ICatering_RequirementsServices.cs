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
    public interface ICatering_RequirementsServices : IClientAdminGenericService<Catering_Requirements>
    {

        List<GetCatMenuListDto> GetCateringReqMenu(string ConnectionString, long CatererId);
        Task<List<Catering_RequirementsDto>> GetCatering_RequirementsList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<Catering_Requirements>> GET_CateReqList(string ConnectionString, long bookingDetailId);
        Task<Catering_RequirementsDto> GET_CateReqAndMenuItemById(string ConnectionString, long id);
        Task<List<CatererRequirementInvoiceDto>> CateringRequirementViewAsPDF(string ConnectionString, long bookingDetailId);
    }
}
