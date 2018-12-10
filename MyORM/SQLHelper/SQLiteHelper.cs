using System;
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
            if (cmd != null)
                cmd.Dispose();
            cmd = new SQLiteCommand(sql, con);
            return cmd.ExecuteNonQuery();
        }

        public void ShutDown()
        {
            if (cmd != null)
                cmd.Dispose();
            con.Dispose();
        }

        public bool IsClose()
        {
            return con.State == ConnectionState.Closed;
        }
    }
}
