using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace MyORM
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

        public  void ShutDown()
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
