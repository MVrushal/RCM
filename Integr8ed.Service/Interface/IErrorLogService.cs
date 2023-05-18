using Integr8ed.Data.DbModel;
using Integr8ed.Service.BaseInterface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Integr8ed.Service.Interface
{
    public interface IErrorLogService : IGenericService<ErrorLog>
    {
        void AddErrorLog(System.Exception ex, string appType);
    }
}
