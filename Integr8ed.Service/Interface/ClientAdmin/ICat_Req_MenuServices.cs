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
    public interface ICat_Req_MenuServices : IClientAdminGenericService<Cat_Req_Menu>
    {
        Task<List<Cat_Req_MenuDto>> GetCat_Req_MenuList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<Cat_Req_Menu>> GetCat_Req_MenuListByCatReqId(string ConnectionString, long cat_ReqId);
}
}
