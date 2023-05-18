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
    public class EquipmentRepository : ClientAdminGenericRepository<Equipment>, IEquipServices
    {
        private readonly ClientAdminDbContext _context;
        public EquipmentRepository(ClientAdminDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<EquiptDto>> GetEquipmentList(string ConnectionString, SqlParameter[] paraObjects)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var dataSet = await db.GetClientQueryDatatableAsync(SpConstants_ClientAdmin.GetEquipList, paraObjects);
                return Common.ConvertDataTable<EquiptDto>(dataSet.Tables[0]);
            }


        }
        public async Task<List<Equipment>> GET_EuipmentReqList(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {

                var result = await db.Equipment.Where(x => !x.IsDelete).ToListAsync();
                return result;
            }
        }

        public async Task<List<EquipmentRequiredForBookingDto>> GET_EuipmentList(string ConnectionString)
        {
            var dbConnection = new SqlConnection(ConnectionString);
            var OptionBuilder = new DbContextOptionsBuilder<ClientAdminDbContext>();
            OptionBuilder.UseSqlServer(dbConnection);
            using (var db = new ClientAdminDbContext(OptionBuilder.Options))
            {
                var result = await (from eq in db.Equipment
                                    join seq in db.StandardEquipment on eq.EquipId equals seq.Id
                                    where !eq.IsDelete && !seq.IsDelete && eq.Title != "0"
                                    select new EquipmentRequiredForBookingDto
                                    {
                                        EqupTitle = seq.Title,
                                        Description = seq.Description,
                                        EquipId = seq.Id
                                    }).ToListAsync();

                return result;
            }
        }

    }
}
