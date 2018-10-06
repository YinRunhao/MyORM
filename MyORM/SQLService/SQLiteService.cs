using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    public class SQLiteService:SQLService
    {
        private static string connectionString;

        public static void SetConnection(string conStr)
        {
            connectionString = conStr;
        }

        public SQLiteService()
        {
            // helper.ConnectionString = connectionString;
            helper = new SQLiteHelper(connectionString);
            stringBuilder = new SQLiteStringBulider();
        }

        protected override void OpenConnection()
        {
            if (helper.IsClose())
            {
                helper = new SQLiteHelper(connectionString);
            }
        }
    }
}
