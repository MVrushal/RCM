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
    public interface IEquipmentRequiredForBookingServices : IClientAdminGenericService<EquipmentRequiredForBooking>
    {
        Task<List<EquipmentRequiredForBookingDto>> GetEquipmentRequiredForBookingList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<EquipmentRequiredForBooking>> GET_EuipmentReqForBookingList(string ConnectionString, long bookingDetailId);
        Task<decimal> CheckISEquipmentAvalable(string v, SqlParameter[] sp);
        Task<List<CheckEquipmetIsavailable>> CheckBookingStatusEquipmentAvalable(string v, SqlParameter[] sp);
    }
}
