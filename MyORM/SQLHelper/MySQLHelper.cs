using System;
using System.Collections.Generic;
using System.Data;
using MySql.Data.MySqlClient;

namespace MyORM.DbHelper
{
    class MySQLHelper : ISQLHelper
    {
        private MySqlConnection con;
        private MySqlCommand cmd;

        public MySQLHelper(string connectionString)
        {
            try
            {
                con = new MySqlConnection(connectionString);
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

        public  int DoUpdate(string sql)
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
            cmd = new MySqlCommand(sql, con);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    temp = parameters[i];
                    if (string.IsNullOrEmpty(temp.Key))
                    {
                        continue;
                    }
                    MySqlParameter sqlPara = new MySqlParameter("@" + temp.Key, temp.Value);
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
            cmd = new MySqlCommand(sql, con);
            if (null != conditions)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    temp = conditions[i];
                    MySqlParameter sqlPara = new MySqlParameter("@" + temp.Key, temp.Value);
                    cmd.Parameters.Add(sqlPara);
                }
            }
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
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
