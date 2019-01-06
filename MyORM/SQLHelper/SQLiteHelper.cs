using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace MyORM.DbHelper
{
    class SQLiteHelper : ISQLHelper
    {
        private SQLiteConnection con;
        private SQLiteCommand cmd;

        public SQLiteHelper(string connectionString)
        {
            try
            {
                con = new SQLiteConnection(connectionString);
                con.Open();
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public DataTable DoSelect(string sql)
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
            cmd = new SQLiteCommand(sql, con);
            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    temp = parameters[i];
                    if (string.IsNullOrEmpty(temp.Key))
                    {
                        continue;
                    }
                    SQLiteParameter sqlPara = new SQLiteParameter("@"+temp.Key,temp.Value);
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
            cmd = new SQLiteCommand(sql, con);
            if (null != conditions)
            {
                for (int i = 0; i < conditions.Length; i++)
                {
                    temp = conditions[i];
                    SQLiteParameter sqlPara = new SQLiteParameter("@"+temp.Key,temp.Value);
                    cmd.Parameters.Add(sqlPara);
                }
            }
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
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
