using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Data.Extensions
{
    public static class DbContextExtensions
    {
        public static async Task<DataSet> GetClientQueryDatatableAsync(this Microsoft.EntityFrameworkCore.DbContext context, string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure)
        {
            try
            {

                using (DbCommand cmd = context.Database.GetDbConnection().CreateCommand())
                {
                    cmd.Connection = context.Database.GetDbConnection();
                    cmd.CommandType = type;
                    cmd.CommandTimeout = 60;
                    if (sqlParam != null)
                    { cmd.Parameters.AddRange(sqlParam); }
                    cmd.CommandText = sqlQuery;
                    using (DbDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        await Task.Run(() => dataAdapter.Fill(ds));
                        return ds;
                    }
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
        public static async Task<DataSet> GetQueryDatatableAsync(this Microsoft.EntityFrameworkCore.DbContext context, string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure)
        {
            using (DbCommand cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.Connection = context.Database.GetDbConnection();
                cmd.CommandType = type;
                if (sqlParam != null)
                { cmd.Parameters.AddRange(sqlParam); }
                cmd.CommandText = sqlQuery;
                using (DbDataAdapter dataAdapter = new SqlDataAdapter())
                {
                    dataAdapter.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    await Task.Run(() => dataAdapter.Fill(ds));
                    return ds;
                }
            }
        }
        public static DataSet GetQueryDatatable(this Microsoft.EntityFrameworkCore.DbContext context, string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure)
        {
            using (DbCommand cmd = context.Database.GetDbConnection().CreateCommand())
            {
                cmd.Connection = context.Database.GetDbConnection();
                cmd.CommandType = type;
                if (sqlParam != null)
                { cmd.Parameters.AddRange(sqlParam); }
                cmd.CommandText = sqlQuery;
                using (DbDataAdapter dataAdapter = new SqlDataAdapter())
                {
                    dataAdapter.SelectCommand = cmd;
                    DataSet ds = new DataSet();
                    dataAdapter.Fill(ds);
                    return ds;
                }
            }
        }
    }
}
