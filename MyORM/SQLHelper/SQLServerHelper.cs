﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MyORM
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
            if (cmd != null)
                cmd.Dispose();
            cmd = new SqlCommand(sql, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            return ds.Tables[0];
        }

        public int DoUpdate(string sql)
        {
            if (cmd != null)
                cmd.Dispose();
            cmd = new SqlCommand(sql, con);
            return cmd.ExecuteNonQuery();
        }

        public void ShutDown()
        {
            if (cmd != null)
                cmd.Dispose();
            con.Close();
        }

        public bool IsClose()
        {
            return con.State == ConnectionState.Closed;
        }
    }
}