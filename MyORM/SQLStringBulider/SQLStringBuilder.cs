using MyORM.Attributes;
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
    /// 生成对应的SQL语句（基于MYSQL，若扩展其他数据库需要重写某些方法）
    /// </summary>
    abstract class SQLStringBuilder:ISQLStringBuilder
    {
        /// <summary>
        /// 查询该表下所有数据
        /// </summary>
        /// <typeparam name="T">类型名</typeparam>
        /// <returns></returns>
        public virtual string SelectAllString<T>() where T : ModelBase
        {
            var type = typeof(T);
            string name = "";
            if (type.IsDefined(typeof(MyTableAttribute), false))
            {
                var attrs = type.GetCustomAttributes(false);
                foreach (var att in attrs)
                {
                    MyTableAttribute tableAttr = (MyTableAttribute)att;
                    if (tableAttr != null)
                    {
                        name = tableAttr.Name;
                    }
                }
            }
            else
            {
                name = type.Name;
            }
            return "select * from " + name;
        }

        /// <summary>
        /// 以主键为条件查询某个表，只返回第一条数据
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="primaryKeys">主键对应的名字和值</param>
        /// <returns></returns>
        public virtual string SelectOneRowByID(string TableName, params KeyValuePair<string, string>[] primaryKeys)
        {
            StringBuilder sb = new StringBuilder();
            string[] primaryKeyNames = new string[primaryKeys.Count()];
            int i = 0;
            foreach (var primaryKey in primaryKeys)
            {
                primaryKeyNames[i] = primaryKey.Key;
                i++;
            }
            sb.Append(SelectByCondition(TableName, primaryKeys,primaryKeyNames));
            sb.Append(" limit 1");
            return sb.ToString();
        }

        /// <summary>
        /// 对表进行条件查询
        /// </summary>
        /// <param name="TableName">表明</param>
        /// <param name="conditions">查询条件</param>
        /// <returns></returns>
        public virtual string SelectByCondition(string TableName, KeyValuePair<string, string>[] conditions, string[] orderBy = null, string orderType = "asc")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from " + TableName + " where ");
            foreach (var kv in conditions)
            {
                if (kv.Value.ToLower() != "null")
                    sb.Append(kv.Key + "='" + kv.Value + "' and ");
                else
                {
                    sb.Append(kv.Key + "=" + kv.Value + " and ");
                }
            }
            sb.Append("1=1 order by ");
            bool flag = false;
            foreach (string props in orderBy)
            {
                if (flag)
                {
                    sb.Append(",");
                }
                else
                {
                    flag = true;
                }
                sb.Append(props);
            }
            sb.Append(" "+orderType);
            return sb.ToString();
        }

        /// <summary>
        /// 更新某个表的某条数据 
        /// </summary>
        /// <param name="TableName">要更新的表</param>
        /// <param name="values">对象的各种属性和值</param>
        /// <param name="conditions">主键值</param>
        /// <returns></returns>
        public virtual string UpdateString(string TableName, KeyValuePair<string, object>[] values, params KeyValuePair<string, object>[] conditions)
        {
            string whereString = BuildWhereString(conditions);
            StringBuilder sb = new StringBuilder();
            sb.Append("update " + TableName + " set ");
            foreach (var val in values)
            {
                sb.Append(val.Key + "=" + "@"+val.Key + ",");
            }
            sb = sb.Remove(sb.Length - 1, 1);
            sb.Append(whereString);
            return sb.ToString();
        }

        /// <summary>
        /// 根据条件创建where字符串
        /// </summary>
        /// <param name="conditions"></param>
        /// <returns></returns>
        private static string BuildWhereString(params KeyValuePair<string, object>[] conditions)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(" where ");
            foreach (var kv in conditions)
            {
                sb.Append(kv.Key + "=@" + kv.Key + " and ");
            }
            sb.Append("1=1");
            return sb.ToString();
        }

        public virtual string InsertString(string TableName, KeyValuePair<string, string>[] values)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("insert into " + TableName + "(");
            foreach (var val in values)
            {
                if (!string.IsNullOrEmpty(val.Key))
                {
                    sb.Append(val.Key);
                    sb.Append(",");
                }
            }
            sb = sb.Remove(sb.Length - 1, 1);
            sb.Append(") values('");
            foreach (var val in values)
            {
                if (!string.IsNullOrEmpty(val.Value))
                {
                    if (val.Value.ToLower() == "null")
                    {
                        sb = sb.Remove(sb.Length - 1, 1);
                        sb.Append("null,'");
                        continue;
                    }
                    sb.Append(val.Value);
                    sb.Append("','");
                }
            }
            sb = sb.Remove(sb.Length - 2, 2);
            sb.Append(")");
            return sb.ToString(); ;
        }

        public virtual string DeleteString(string TableName, params KeyValuePair<string, object>[] conditions)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("delete from " + TableName);
            sb.Append(BuildWhereString(conditions));

            return sb.ToString(); ;
        }

        public virtual string SelectPageList(string Table, int pageSize, int nowPage, string[] orderBy = null,string orderType = "asc")
        {
            return "select * from " + Table + ",(select count(*) as cnt from " + Table + ") as T " +OrderByString(orderType,orderBy)+ LimitString(pageSize, nowPage);
        }

        public virtual string SelectPageListWithCondition(string Table, int pageSize, int nowPage,KeyValuePair<string, string>[] conditions, string[] orderBy = null, string orderType = "asc")
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("select * from " + Table + ",(select count(*) as cnt from " + Table + ") as T" + " where ");
            foreach (var kv in conditions)
            {
                if (kv.Value.ToLower() != "null")
                    sb.Append(kv.Key + "='" + kv.Value + "' and ");
                else
                {
                    sb.Append(kv.Key + "=" + kv.Value + " and ");
                }
            }
            sb.Append("1=1 ");
            return sb.ToString()+OrderByString(orderType, orderBy) + LimitString(pageSize, nowPage);
        }

        private static string LimitString(int pageSize, int nowPage)
        {
            int offset = nowPage * (pageSize - 1);
            return " limit " + offset + "," + pageSize;
        }

        private static string SelectMany_Base(string TableName)
        {
            return "select * from " + TableName + " where 1=1 and ";
        }

        /// <summary>
        /// 根据C#表达式构造SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="express"></param>
        /// <returns></returns>
        public virtual string SelectMany<T>(Expression<Func<T, bool>> express,string TableName)
        {
            //string TableName = typeof(T).Name;
            
            var ex = express.Body;
            string sql = SelectMany_Base(TableName);
            sql += ExpressionHandle.DealExpression(ex);
            return sql;
        }

        public virtual string OrderByString(string orderType, string[] property)
        {
            string sql = "order by ";
            if (orderType.ToLower().CompareTo("asc") == 0 || orderType.ToLower().CompareTo("desc") == 0)
            {
                bool flag = true;
                if (property==null || property.Length == 0)
                {
                    return "";
                }
                foreach (string val in property)
                {
                    if (flag)
                    {
                        flag = false;
                        sql += val;
                    }
                    else
                    {
                        sql += ","+val;
                    }
                }
                sql = sql + " "+orderType.ToLower();
                return sql;
            }
            else
                throw new ArgumentException("排序类型:"+orderType+"不正确");
        }

        public virtual string SelectPageListWithCondition<T>(string Table, int pageSize, int nowPage, Expression<Func<T, bool>> condition, string[] orderBy = null, string orderType = "asc")
        {
            StringBuilder sb = new StringBuilder();
            string whereStr = "";
            sb.Append("select * from " + Table + ",(select count(*) as cnt from " + Table + ") as T" + " where ");
            if(condition != null)
            {
                whereStr = ExpressionHandle.DealExpression(condition.Body);
                sb.Append(whereStr);
                sb.Append(" and ");
            }
            sb.Append("1=1 ");
            return sb.ToString() + OrderByString(orderType, orderBy) + LimitString(pageSize, nowPage);
        }

        /// <summary>
        /// 不同的数据库用的函数不一样，必须由派生类自己实现
        /// </summary>
        /// <param name="Table"></param>
        /// <returns></returns>
        public virtual string SelectLastInsertRow(string Table,string primaryKey)
        {
            throw new NotImplementedException();
        }
    }
}
