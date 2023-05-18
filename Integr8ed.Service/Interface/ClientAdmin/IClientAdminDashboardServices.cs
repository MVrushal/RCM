using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface.BaseInterface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Interface.ClientAdmin
{
  public   interface IClientAdminDashboardServices //: IClientAdminGenericService<T>
    {
        
         Task<List<BookingDetailsForGridDto>> GetBookingCountDetail(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<ToolTip>> GetBookingCountToolTipdetail(string ConnectionString, string Name,int BookingType);
        Task<List<ToolTip>> GetMeetingCountToolTipdetail(string ConnectionString, string Name);
        
        Task<DashboardDto> GetClientAdminDashboardData(string ConnectionString, SqlParameter[] paraObjects);
    }
}
