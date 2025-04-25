using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TC_Project
{
    public class SqlDataAccess
    {
        public string connectionString = "";
        public SqlDataAccess()
        {
            connectionString = ConfigurationManager.ConnectionStrings["QMDS_Connection"].ConnectionString;
        }

        public DataSet getDataFromProcedure(string pro, string tableName, params SqlParameter[] cmdParam)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                conn.Open();
                da.SelectCommand = new SqlCommand(pro, conn);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < cmdParam.Length; i++)
                {
                    da.SelectCommand.Parameters.Add(cmdParam[i]);
                }
                if (tableName != "")
                {
                    da.Fill(ds, tableName);
                }
                else
                {
                    da.Fill(ds);
                }
            }
            catch (SqlException sqlex)
            {
                throw sqlex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public DataSet getDataFromProcedure(string pro, string tableName)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                conn.Open();
                da.SelectCommand = new SqlCommand(pro, conn);
                da.SelectCommand.CommandType = CommandType.StoredProcedure;
                if (tableName != "")
                {
                    da.Fill(ds, tableName);
                }
                else
                {
                    da.Fill(ds);
                }
            }
            catch (SqlException sqlex)
            {
                throw sqlex;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public object GetValueFromFuntion(string fun, params SqlParameter[] cmdParam)
        {
            SqlCommand cmd;
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                cmd = new SqlCommand(fun, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                for (int i = 0; i < cmdParam.Length; i++)
                {
                    cmd.Parameters.Add(cmdParam[i]);

                }
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch (SqlException exx)
            {
                throw exx;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return cmd.Parameters["@Result"].Value;
        }
        public DataSet getDataFromSql(string sql, string TableName, params SqlParameter[] cmdParam)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                conn.Open();
                da.SelectCommand = new SqlCommand(sql, conn);

                for (int i = 0; i < cmdParam.Length; i++)
                {
                    da.SelectCommand.Parameters.Add(cmdParam[i]);
                }
                if (TableName != "")
                {
                    da.Fill(ds, TableName);
                }
                else
                {
                    da.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public DataSet getDataFromSql(string sql, string TableName)
        {
            DataSet ds = new DataSet();
            SqlConnection conn = new SqlConnection(connectionString);
            SqlDataAdapter da = new SqlDataAdapter();
            try
            {
                conn.Open();
                da.SelectCommand = new SqlCommand(sql, conn);
                if (TableName != "")
                {
                    da.Fill(ds, TableName);
                }
                else
                {
                    da.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
            return ds;
        }
        public bool DbCommand(string sql)
        {
            SqlConnection conn = new SqlConnection(connectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
            }
        }
    }
}
