using MyORM.DbHelper;
using MyORM.DbStringBuilder;

namespace MyORM.DbService
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
