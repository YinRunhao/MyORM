using System.Data;

namespace MyORM.DbHelper
{
    public interface ISQLHelper
    {
        DataTable DoSelect(string sql);
        int DoUpdate(string sql);
        bool IsClose();
        void ShutDown();
       // string ConnectionString { get; set; }
    }
}
