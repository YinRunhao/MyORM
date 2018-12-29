using System;
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
            //this.sql = sql;
            if (cmd != null)
                cmd.Dispose();
            cmd = new MySqlCommand(sql,con);
            MySqlDataAdapter adapter = new MySqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0];
        }

        public  int DoUpdate(string sql)
        {
            if (cmd != null)
                cmd.Dispose();
            cmd = new MySqlCommand(sql,con);
            return cmd.ExecuteNonQuery();
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
    }
}
