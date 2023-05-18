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
    public interface IBookingNotificationServices : IClientAdminGenericService<BookingDetails>
    {
        Task<List<BookingDetailsDto>> GetBookingList(string ConnectionString);

        Task<bool> DeleteBookingByCompanyCodeANDId(string ConnectionString, SqlParameter[] paraObjects);
    }
}
