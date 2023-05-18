using Integr8ed.Data.DbModel.SuperAdmin;
using Integr8ed.Service.BaseInterface;
using Integr8ed.Service.Dto;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface
{
   public  interface ICompnayService:  IGenericService<Company>
    {
        Task<List<CompanyDto>> GetCompanyList(SqlParameter[] paraObjects);
        Task<bool>  CreateStoreProcedures(SqlParameter[] paraObjects);
    
    }
}