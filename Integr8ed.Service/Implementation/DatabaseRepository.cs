using Integr8ed.Data;
using Integr8ed.Data.DbModel.SuperAdmin;
using Integr8ed.Service.BaseService;
using Integr8ed.Service.Dto;
using Integr8ed.Service.Interface;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Integr8ed.Service.Implementation
{


    public class DatabaseRepository : GenericRepository<Database>, IDatabaseService
    {
        private readonly ApplicationDbContext _context;
        public DatabaseRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }
        public string CreateDBComapnyWise(DatabaseParamDto DBParam,string CurrentConnectionString)
        {
            string sqlCreateDBQuery;
            SqlConnection tmpConn = new SqlConnection();
            tmpConn.ConnectionString = "SERVER = " + DBParam.ServerName +
                                 "; DATABASE = master; User ID=" + DBParam.UserID + ";Password= " + DBParam.Password;
            sqlCreateDBQuery = " CREATE DATABASE "
                               + DBParam.DatabaseName
                               + " ON PRIMARY "
                               + " (NAME = " + DBParam.DataFileName + ", "
                               + " FILENAME = '" + DBParam.DataPathName + "', "
                               + " SIZE = 3MB,"
                               + " FILEGROWTH =" + DBParam.DataFileGrowth + ") "
                               + " LOG ON (NAME =" + DBParam.LogFileName + ", "
                               + " FILENAME = '" + DBParam.LogPathName + "', "
                               + " SIZE = 1MB, "
                               + " FILEGROWTH =" + DBParam.LogFileGrowth + ") ";
            SqlCommand myCommand = new SqlCommand(sqlCreateDBQuery, tmpConn);
            DBParam.Password = "Password123#";
            string SqlLoginCreate = "CREATE LOGIN " + DBParam.DatabaseName + " WITH PASSWORD ='" + DBParam.Password + "'";

        
            SqlCommand myCommandForLogin = new SqlCommand(SqlLoginCreate, tmpConn);

            try
            {
                tmpConn.Open();

                /* KDK Executaed for create New DB*/
               var dbresult= myCommand.ExecuteNonQuery();

             
                /*KDK executaed for create new Database Login*/
                myCommandForLogin.ExecuteNonQuery();

                /*KDK executaed for create table in new Database */
                string TableScript = File.ReadAllText(Directory.GetCurrentDirectory() + @"\wwwroot\DbScript\Tables.sql");
                TableScript = TableScript.Replace("{dynamicDatanase}", DBParam.DatabaseName);
                SqlCommand commandForTable = new SqlCommand(TableScript, tmpConn);
                commandForTable.ExecuteNonQuery();
                tmpConn.Close();
                /*KDK  created table in new Database */



               /*  creating role & Sp  in new Database */
               SqlConnection sqlConnection = new SqlConnection(CurrentConnectionString);
                SqlCommand commandForSp = new SqlCommand("createsp", sqlConnection);
                commandForSp.CommandType = CommandType.StoredProcedure;
                commandForSp.Parameters.Add(
                         new SqlParameter("@dbName", DBParam.DatabaseName));
                sqlConnection.Open();
                commandForSp.ExecuteNonQuery();
                sqlConnection.Close();

                /*KDK  created role & Sp  in new Database */
            }
            catch (System.Exception ex)
            {   
                throw ex;
            }
            finally
            {
                tmpConn.Close();
            }

            return "Sucess";
        }



        public string GetConnectionStringByCompanyCode(string Comanycode)
        {
            try
            {
                var data = _context.Databases.FirstOrDefault(m => m.ComanyCode == Comanycode);
                
                return data != null ? data.Dbconnectionstring : "";
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }

}
