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
  public  interface IEquipServices
   : IClientAdminGenericService<Equipment>
    {
        Task<List<EquiptDto>> GetEquipmentList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<Equipment>> GET_EuipmentReqList(string ConnectionString);
        Task<List<EquipmentRequiredForBookingDto>> GET_EuipmentList(string ConnectionString);
    }
}
