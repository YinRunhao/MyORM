using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    /// <summary>
    /// 产生对应的SQL语句(MySQL)
    /// </summary>
    internal class MySQLStringBuilder:SQLStringBuilder
    {
        public override string SelectLastInsertRow(string Table,string primaryKey)
        {
            string ret = "select * from " + Table + " where " + Table + "." + primaryKey + "=last_insert_id();";
            return ret;
        }
    }
}
