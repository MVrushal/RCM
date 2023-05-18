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
   public interface IEntryTypeServices : IClientAdminGenericService<Entry_Type>
    {
        Task<List<EntryTypeDto>> GetEntryTypeList(string ConnectionString, SqlParameter[] paraObjects);
        List<Entry_Type> GET_EntryTypeList(string ConnectionString,long BranchId);
    }
}
