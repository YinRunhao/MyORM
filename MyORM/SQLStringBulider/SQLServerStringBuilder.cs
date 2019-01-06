using MyORM.ExpressionTools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MyORM.DbStringBuilder
{
    /// <summary>
    /// 生成对应的SQL语句(SQLserver)
    /// </summary>
    internal class SQLServerStringBuilder:SQLStringBuilder
    {
        public override string SelectOneRowByID(string TableName, string[] primaryKeys)
        {
            string sql = SelectByCondition(TableName, primaryKeys, primaryKeys);
            sql = sql.Insert(6," top 1");
            return sql;
        }

        public override string SelectPageList(string Table, int pageSize, int nowPage, string[] orderBy = null, string orderType = "asc")
        {
            int star = (nowPage - 1) * pageSize + 1;
            int end = star + pageSize-1;           
            return "select * from(select ROW_NUMBER()over("+OrderByString(orderType,orderBy)+") rowID,* from "+Table+")  as tbq,(select count(*) cnt from "+Table+") as tb where tbq.rowID between "+star+" and "+end+";";
        }
        public override string SelectPageListWithCondition(string Table, int pageSize, int nowPage, string[] conditions, string[] orderBy = null, string orderType = "asc")
        {
            string whereStr = "";
            StringBuilder sb = new StringBuilder();
            sb.Append(" where 1=1");
            foreach (string item in conditions)
            {
                sb.Append(" and ");
                sb.Append(item);
                sb.Append("=@");
                sb.Append(item);
            }
            whereStr = sb.ToString();
            int star = (nowPage - 1) * pageSize + 1;
            int end = star + pageSize;
            return "select * from(select ROW_NUMBER()over(" + OrderByString(orderType, orderBy)+") rowID,* from " + Table + " "+whereStr+")  as tbq,(select count(*) cnt from " + Table + ") as tb where tbq.rowID between " + star + " and " + end + ";";
        }

        public override KeyValuePair<string,object>[] SelectPageListWithCondition<T>(out string sql ,string Table, int pageSize, int nowPage, Expression<Func<T, bool>> condition, string[] orderBy = null, string orderType = "asc")
        {
            KeyValuePair<string, object>[] ret = null;
             string whereStr = "";
             StringBuilder sb = new StringBuilder();
             sb.Append(" where ");
             if (condition != null)
             {
                ret = ExpressionHandle.DealExpression(out whereStr, condition.Body);
                 sb.Append(whereStr);
                 sb.Append(" and ");
             }
             sb.Append("1=1 ");
             whereStr = sb.ToString();
             int star = (nowPage - 1) * pageSize + 1;
             int end = star + pageSize -1;
            sql= "select * from(select ROW_NUMBER()over(" + OrderByString(orderType, orderBy) + ") rowID,* from " + Table + " " + whereStr + ")  as tbq,(select count(*) cnt from " + Table + ") as tb where tbq.rowID between " + star + " and " + end + ";";
            return ret;
        }

        public override string SelectLastInsertRow(string Table, string primaryKey)
        {
            string ret = "select * from " + Table + " where " + Table + "." + primaryKey + "=@@IDENTITY;";
            return ret;
        }
    }
}
