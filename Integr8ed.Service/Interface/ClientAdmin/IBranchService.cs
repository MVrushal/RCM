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
    public interface IBranchService : IClientAdminGenericService<BranchMaster>
    {

        List<BranchMaster> getBranchList(string connectionString, string userID);

        Task<List<BranchDto>> BranchMasterList(string ConnectionString, SqlParameter[] paraObjects);
        List<Room_TypeDto> GetRoomList(string ConnectionString, long Id);
        public int GetBookingRequestCounts(string ConnectionString, string userID, long BranchId);
       

    }
}
