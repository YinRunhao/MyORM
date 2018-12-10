using MyORM.DbHelper;
using MyORM.DbStringBuilder;

namespace MyORM.DbService
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
