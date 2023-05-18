using Integr8ed.Data.DbContext.ClientAdminContext;
using Integr8ed.Data.DbModel.ClientAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Implementation.BaseService;
using Integr8ed.Service.Interface.ClientAdmin;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class BookingRequestRepository : ClientAdminGenericRepository<BookingRequest>, IbookingRequestService
    {
        private readonly ClientAdminDbContext _context;
        public BookingRequestRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<BookingDetailsDto>> GetRequestList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetRequestList, paraObjects);
                return Common.ConvertDataTable<BookingDetailsDto>(dataSet.Tables[0]);
            }


        }

    } 
    }
