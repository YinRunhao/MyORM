using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyORM
{
    /// <summary>
    /// 生成对应的SQL语句(SQLserver)
    /// </summary>
    internal class SQLServerStringBuilder:SQLStringBuilder
    {
        public override string SelectOneRowByID(string TableName, params KeyValuePair<string, string>[] primaryKeys)
        {
            string[] primaryKeyNames = new string[primaryKeys.Count()];
            int i = 0;
            foreach (var primaryKey in primaryKeys)
            {
                primaryKeyNames[i] = primaryKey.Key;
                i++;
            }
            string sql = SelectByCondition(TableName, primaryKeys,primaryKeyNames);
            sql = sql.Insert(6," top 1");
            return sql;
        }

        public override string SelectPageList(string Table, int pageSize, int nowPage, string[] orderBy = null, string orderType = "asc")
        {
            int star = (nowPage - 1) * pageSize + 1;
            int end = star + pageSize-1;           
            return "select * from(select ROW_NUMBER()over("+OrderByString(orderType,orderBy)+") rowID,* from "+Table+")  as tbq,(select count(*) cnt from "+Table+") as tb where tbq.rowID between "+star+" and "+end+";";
        }
        public override string SelectPageListWithCondition(string Table, int pageSize, int nowPage, KeyValuePair<string, string>[] conditions, string[] orderBy = null, string orderType = "asc")
        {
            string whereStr = "";
            StringBuilder sb = new StringBuilder();
            sb.Append(" where ");
            foreach (var kv in conditions)
            {
                if (kv.Value.ToLower() != "null")
                    sb.Append(kv.Key + "='" + kv.Value + "' and ");
                else
                {
                    sb.Append(kv.Key + "=" + kv.Value + " and ");
                }
            }
            sb.Append("1=1");
            whereStr = sb.ToString();
            int star = (nowPage - 1) * pageSize + 1;
            int end = star + pageSize;
            return "select * from(select ROW_NUMBER()over(" + OrderByString(orderType, orderBy)+") rowID,* from " + Table + " "+whereStr+")  as tbq,(select count(*) cnt from " + Table + ") as tb where tbq.rowID between " + star + " and " + end + ";";
        }

        public override string SelectPageListWithCondition<T>(string Table, int pageSize, int nowPage, Expression<Func<T, bool>> condition, string[] orderBy = null, string orderType = "asc")
        {
            string whereStr = "";
            StringBuilder sb = new StringBuilder();
            sb.Append(" where ");
            if (condition != null)
            {
                whereStr = ExpressionHandle.DealExpression(condition.Body);
                sb.Append(whereStr);
                sb.Append(" and ");
            }
            sb.Append("1=1 ");
            whereStr = sb.ToString();
            int star = (nowPage - 1) * pageSize + 1;
            int end = star + pageSize -1;
            return "select * from(select ROW_NUMBER()over(" + OrderByString(orderType, orderBy) + ") rowID,* from " + Table + " " + whereStr + ")  as tbq,(select count(*) cnt from " + Table + ") as tb where tbq.rowID between " + star + " and " + end + ";";
        }
    }
}
