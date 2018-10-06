using MyORM;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    public class MySQLService : SQLService
    {
        private static string connectionString;

        public static void SetConnection(string conStr)
        {
            connectionString = conStr;
        }

        public MySQLService()
        {
           // helper.ConnectionString = connectionString;
            helper = new MySQLHelper(connectionString);
            stringBuilder = new MySQLStringBuilder();
        }

        protected override void OpenConnection()
        {
            if (helper.IsClose())
                helper = new MySQLHelper(connectionString);
        }
    }

    
}
