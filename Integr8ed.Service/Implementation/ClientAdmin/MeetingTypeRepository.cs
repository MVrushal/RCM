using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
   public class MeetingTypeRepository : ClientAdminGenericRepository<MeetingType>, IMeetingTypeServices { 
    private readonly ClientAdminDbContext _context;
    public MeetingTypeRepository(ClientAdminDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<List<MeetingTypeDto>> GetMeetingList(string ConnectionString, SqlParameter[] paraObjects)
    {
        var dbConnection = new SqlConnection(ConnectionString);
        var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
        OptionBuilder.UseSqlServer(dbConnection);
        using (var db = new ClientAdminDbContext(OptionBuilder.Options))
        {

            var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetMeetingList, paraObjects);
            return Common.ConvertDataTable<MeetingTypeDto>(dataSet.Tables[0]);
        }


    }

        public async Task<List<MeetingType>> GetMeetingListForDropDown(string ConnectionString,long BranchId)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.MeetingTypes.Where(x => !x.IsDelete && x.BranchId== BranchId).ToListAsync();
                return result;
            }
        }
    }
}
