using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace MyORM.DbHelper
{
    internal class SQLServerHelper : ISQLHelper
    {
        private SqlConnection con;
        private SqlCommand cmd;
        public SQLServerHelper(string connectionStr)
        {
            try
            {
                con = new SqlConnection(connectionStr);
                con.Open();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public  DataTable DoSelect(string sql)
        {
            return DoSelect(sql,null);
        }

        public int DoUpdate(string sql)
        {
            return DoUpdate(sql,null);
        }

        public void ShutDown()
        {
            if (cmd != null)
                cmd.Dispose();
            con.Dispose();
            con = null;
        }

        public bool IsClose()
        {
            return (null == con) || (con.State == ConnectionState.Closed);
        }

        public int DoUpdate(string sql, KeyValuePair<string, object>[] parameters)
        {
            KeyValuePair<string, object> temp;
            if (cmd != null)
                cmd.Dispose();
            cmd = new SqlCommand(sql, con);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    temp = parameters[i];
                    if (string.IsNullOrEmpty(temp.Key))
                    {
                        continue;
                    }
                    SqlParameter sqlPara = new SqlParameter("@" + temp.Key, temp.Value);
                    cmd.Parameters.Add(sqlPara);
                }
            }
            return cmd.ExecuteNonQuery();
        }

        public DataTable DoSelect(string sql, KeyValuePair<string, object>[] conditions)
        {
            KeyValuePair<string, object> temp;
            DataTable ret = null;
            if (cmd != null)
                cmd.Dispose();
            cmd = new SqlCommand(sql, con);
            if (null != conditions)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    temp = conditions[i];
                    SqlParameter sqlPara = new SqlParameter("@" + temp.Key, temp.Value);
                    cmd.Parameters.Add(sqlPara);
                }
            }
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            adapter.Dispose();
            adapter = null;
            ret = ds.Tables[0];

            ds.Dispose();
            ds = null;
            return ret;
        }
    }
}
