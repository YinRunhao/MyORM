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
            if (cmd != null)
                cmd.Dispose();
            cmd = new SQLiteCommand(sql, con);
            SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0];
        }

        public int DoUpdate(string sql)
        {
            /*if (cmd != null)
                cmd.Dispose();
            cmd = new SQLiteCommand(sql, con);*/
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
                    SQLiteParameter sqlPara = new SQLiteParameter("@"+temp.Key,temp.Value);
                    cmd.Parameters.Add(sqlPara);
                }
            }
            return cmd.ExecuteNonQuery();
        }
    }
}
