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
    public interface IBookingDetailServices : IClientAdminGenericService<BookingDetails>
    {
        Task<List<BookingDetailsForGridDto>> GetBookingDetailList(string ConnectionString, SqlParameter[] paraObjects);
        List<BookingDetails> GetBookingDetailForDelete(string ConnectionString);
        Task<List<RoomAvailiblityDto>> GetRoomAvailiblity(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<RoomAvailiblityDto>> GetRoomAvailiblityforDownloadExcel(string ConnectionString, SqlParameter[] paraObjects);
        
        Task<decimal> CalculateBookingCostByDate(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<BookingDetailsDto>> CheckIsBookingDetailAvailable(string ConnectionString, SqlParameter[] paraObjects);
        Task<BookingDetails> CheckBookingDetailMeetingTitle(string ConnectionString, string TitleOfMeeting,long BranchId);

        List<VisitorBookingDto> GetVisitorList(string connectionstring,long BookingId);
        Task<List<AvalDto>> GetbookingTableList(string ConnectionString, SqlParameter[] paraObjects);

        Task<bool> CheckRecurringbookingAvailable(string ConnectionString, SqlParameter[] paraObjects);

        int GetBookingDetails(string ConnectionString, BookingDetailsDto bookingDetails);
        int IsExistBooking(string ConnectionString, BookingDetailsDto bookingDetails);
    }
}
