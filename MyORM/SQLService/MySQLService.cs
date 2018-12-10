using MyORM.DbHelper;
using MyORM.DbStringBuilder;

namespace MyORM.DbService
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
