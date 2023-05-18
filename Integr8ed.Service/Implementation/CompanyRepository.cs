using Integr8ed.Data;
using Integr8ed.Data.DbModel.SuperAdmin;
using Integr8ed.Data.Extensions;
using Integr8ed.Data.Utility;
using Integr8ed.Service.BaseService;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation
{
    public class CompanyRepository : GenericRepository<Company>, ICompnayService
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<CompanyDto>> GetCompanyList(SqlParameter[] paraObjects)
        {
            var dataSet = await _context.GetQueryDatatableAsync(SpConstants.GetCompanyList, paraObjects);
            return Common.ConvertDataTable<CompanyDto>(dataSet.Tables[0]);
        }

        public async Task<bool> CreateStoreProcedures(SqlParameter[] paraObjects)
        {
            try
            {
                var dataSet = await _context.GetQueryDatatableAsync(SpConstants_ClientAdmin.createsp, paraObjects);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }


        }


    }
}