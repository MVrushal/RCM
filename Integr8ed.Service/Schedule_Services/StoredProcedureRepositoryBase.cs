using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Schedule_Services
{
    public interface IStoredProcedureRepositoryBase
    {
        DataSet GetQueryDatatableAsync(string sqlQuery, SqlParameter[] sqlParam, CommandType type);
        List<T> CreateListFromTable<T>(DataTable tbl) where T : new();
        Task<T> GetExecuteNonQueryWithOutParameter<T>(string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure, SqlParameter sqlOutParam = null);
        Task<T> GetExecuteScaler<T>(string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure);
    }

    public class StoredProcedureRepositoryBase : IStoredProcedureRepositoryBase
    {
        string conSting = "Data Source=192.168.1.201;Initial Catalog=ClientAdmin_501;Persist Security Info=True;User ID=NetAdmin;Password=1[$!^(8%]P,7usc";
        public DataSet GetQueryDatatableAsync(string sqlQuery, SqlParameter[] sqlParam, CommandType type)
        {
            try
            {
                
                SqlConnection connection = new SqlConnection(conSting);
                using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
                {
                    cmd.CommandType = type;
                    if (sqlParam != null)
                    { cmd.Parameters.AddRange(sqlParam); }
                    cmd.CommandText = sqlQuery;
                    // cmd.Transaction = GetActiveTransaction();
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter())
                    {
                        dataAdapter.SelectCommand = cmd;
                        DataSet ds = new DataSet();
                        dataAdapter.Fill(ds);
                        return ds;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw ex;
            }
        }

        public List<T> CreateListFromTable<T>(DataTable tbl) where T : new()
        {
            // define return list
            List<T> lst = new List<T>();

            // go through each row
            foreach (DataRow r in tbl.Rows)
            {
                // add to the list
                lst.Add(CreateItemFromRow<T>(r));
            }

            // return the list
            return lst;
        }

        // function that creates an object from the given data row
        public T CreateItemFromRow<T>(DataRow row) where T : new()
        {
            // create a new object
            T item = new T();

            // set the item
            SetItemFromRow(item, row);

            // return 
            return item;
        }

        //public void InsertRecord(string query,string constr)
        //{
        //    SqlConnection con = new SqlConnection(constr);
        //    SqlCommand cmd = new SqlCommand();
        //    SqlDataReader dr = cmd.ExecuteReader();
        //}

      

        public void SetItemFromRow<T>(T item, DataRow row) where T : new()
        {
            // go through each column
            foreach (DataColumn c in row.Table.Columns)
            {
                // find the property for the column
                PropertyInfo p = item.GetType().GetProperty(c.ColumnName);

                // if exists, set the value
                if (p != null && row[c] != DBNull.Value)
                {
                    p.SetValue(item, row[c], null);
                }
            }
        }

        public async Task<T> GetExecuteNonQueryWithOutParameter<T>(string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure, SqlParameter sqlOutParam = null)
        {
          //  string conSting = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection connection = new SqlConnection(conSting);
            using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
            {
                cmd.Connection.Open();
                cmd.CommandTimeout = 60;
                cmd.CommandType = type;
                if (sqlParam != null)
                { cmd.Parameters.AddRange(sqlParam); }
                cmd.CommandText = sqlQuery;
                await cmd.ExecuteNonQueryAsync();

                var outParameterValue = (T)Convert.ChangeType(cmd.Parameters[sqlOutParam.ParameterName].Value, typeof(T));
                cmd.Connection.Close();
                return outParameterValue;

            }
        }

        public async Task<T> GetExecuteScaler<T>(string sqlQuery, SqlParameter[] sqlParam = null, CommandType type = CommandType.StoredProcedure)
        {
           // string conSting = ConfigurationManager.ConnectionStrings["conn"].ConnectionString;
            SqlConnection connection = new SqlConnection(conSting);
            using (SqlCommand cmd = new SqlCommand(sqlQuery, connection))
            {
                cmd.Connection.Open();
                cmd.CommandTimeout = 60;
                cmd.CommandType = type;
                if (sqlParam != null)
                { cmd.Parameters.AddRange(sqlParam); }
                cmd.CommandText = sqlQuery;
                var scalerValue = await cmd.ExecuteScalarAsync();
                return (T)Convert.ChangeType(scalerValue, typeof(T));

            }
        }
    }

    public class ProceduresList
    {
        public const string GetExcelUserList = "GetExcelUserList";
        public const string GetNewEntryList = "GetNewEntryList";
            
    }
}
