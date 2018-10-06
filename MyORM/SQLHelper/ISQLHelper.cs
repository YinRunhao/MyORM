using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace MyORM
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
