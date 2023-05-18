using Integr8ed.Data.DbModel.SuperAdmin;
using Integr8ed.Service.BaseInterface;
using Integr8ed.Service.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface
{
  public  interface IDatabaseService :   IGenericService<Database>
    {
      
        string CreateDBComapnyWise(DatabaseParamDto DBParam, string CurrentConnectionString);
       // Task<List<Database>>
       string  GetConnectionStringByCompanyCode(string ComapnyCode);
    }
}
