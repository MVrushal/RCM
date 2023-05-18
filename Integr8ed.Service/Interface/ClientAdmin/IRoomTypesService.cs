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
  public  interface IRoomTypesService : IClientAdminGenericService<Room_Type>
    {
        Task<List<Room_TypeDto>> GetRoomTypesList(string ConnectionString, SqlParameter[] paraObjects);
        List<StandardEquipment> GET_EuipmentList(string ConnectionString, long BranchId);
        List<Room_Type> GetRoomTypeListForDropDown(string ConnectionString,long BranchId);
        List<Users> GetUserListForDropDown(string ConnectionString, long BranchId);
        Task<List<RoomTypeListDto>> BindDiary(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<DayViewReportDTO>> GetWeeklyDiaryReport(string ConnectionString, SqlParameter[] paraObjects);

        List<Room_Type> GetDiaryRoomList(string ConnetionString, long BranchId);
        bool InsertImage(string ConnectinString, List<RoomTypeImageDto> model);
        RoomTypeImageDto GetImages(string ConnectionString, long Id);
        bool DeleteImage(string Connectionstring, long Id, string ImageName);
        Task<List<RoomTypeListDto>> CheckBooking(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<MonthlyViewDto>> GetMonthlyDiaryList(string ConnectionString, SqlParameter[] paraObjects);
        Task<List<PDFDto>> GetDailyDiaryReportPDF(string ConnectionString, SqlParameter[] paraObjects);    
        Task<List<PDFDto>> GetWeeklyDiaryReportPDF(string ConnectionString, SqlParameter[] paraObjects);    
        Task<List<PDFDto>> GetMonthlyDiaryListPDF(string ConnectionString, SqlParameter[] paraObjects);
        Task<CompanyName> GetOrganizationName(string ConnectionString, SqlParameter[] paraObjects);

    }
}
