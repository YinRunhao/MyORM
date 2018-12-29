using System.Collections.Generic;
using System.Data;

namespace MyORM.DbHelper
{
    public interface ISQLHelper
    {
        DataTable DoSelect(string sql);
        int DoUpdate(string sql);
        int DoUpdate(string sql,KeyValuePair<string,object>[] parameters);
        bool IsClose();

        /// <summary>
        /// dispose connection and set it null
        /// </summary>
        void ShutDown();
       // string ConnectionString { get; set; }
    }
}
