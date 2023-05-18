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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation.ClientAdmin
{
    public class NotesRepository : ClientAdminGenericRepository<Notes>, INotesServices
    {
        private readonly ClientAdminDbContext _context;
        public NotesRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<NotesDto>> GetNotesList(string ConnectionString, SqlParameter[] paraObjects)
        {
            try
            {
                var dbConnection = new SqlConnection(ConnectionString);
                var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
                OptionBuilder.UseSqlServer(dbConnection);
                using (var db = new ClientAdminDbContext(OptionBuilder.Options))
                {

                    var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetNotesList, paraObjects);
                    return Common.ConvertDataTable<NotesDto>(dataSet.Tables[0]);
                }
            }
            catch (Exception ex)
            {
                return new List<NotesDto>();
            }

        }

    }
}
