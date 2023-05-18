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
    public interface ICatererMenuServices : IClientAdminGenericService<CatererMenu>
    {
        Task<List<GetCatMenuListDto>> GetCatMenuList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<GetCatMenuListDto>> GetCatererMenuListByCatererId(string ConnectionString, long catererId);
        List<GetCatMenuListDto> MenuListbyCatererId(string ConnectionString,long CatererId);

        bool DeleteAllMenu(string ConnectionString, long CatererId);
        
    }
}
