using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace MyORM
{
    public class SQLServerService : SQLService
    {
        private static string connectionString;

        public static void SetConnection(string conStr)
        {
            connectionString = conStr;
        }

        public SQLServerService()
        {
           // helper.ConnectionString = connectionString;
            helper = new SQLServerHelper(connectionString);
            stringBuilder = new SQLServerStringBuilder();
        }

        protected override void OpenConnection()
        {
            if (helper.IsClose())
            {
                helper = new SQLServerHelper(connectionString);
            }
        }
    }
}
