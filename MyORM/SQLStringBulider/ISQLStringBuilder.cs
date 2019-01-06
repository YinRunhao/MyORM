using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace MyORM.DbStringBuilder
{
    public interface ISQLStringBuilder
    {
        /// <summary>
        /// 查询该表下所有数据
        /// </summary>
        /// <typeparam name="T">类型名</typeparam>
        /// <returns></returns>
        string SelectAllString<T>()where T:ModelBase;

        /// <summary>
        /// 以主键为条件查询某个表，只返回第一条数据
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="primaryKeys">主键对应的名字和值</param>
        /// <returns></returns>
        string SelectOneRowByID(string TableName, string[] primaryKeys);

        /// <summary>
        /// 对表进行条件查询
        /// </summary>
        /// <param name="TableName">表明</param>
        /// <param name="conditions">查询条件属性名</param>
        /// <returns></returns>
        string SelectByCondition(string TableName, string[] conditions, string[] orderBy=null,string orderType="asc");

        /// <summary>
        /// 更新某个表的某条数据 
        /// </summary>
        /// <param name="TableName">要更新的表</param>
        /// <param name="modifiedProps">对象的各种属性名</param>
        /// <param name="primaryProps">主键属性名</param>
        /// <returns></returns>
        string UpdateString(string TableName, string[] modifiedProps, string[] primaryProps);

        /// <summary>
        /// 构建插入语句
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="values">对象的各种属性名</param>
        /// <returns></returns>
        string InsertString(string TableName, string[] values);

        /// <summary>
        /// 构建删除语句
        /// </summary>
        /// <param name="TableName">表名</param>
        /// <param name="conditions">删除过滤条件涉及的属性名</param>
        /// <returns></returns>
        string DeleteString(string TableName, string[] conditions);

        /// <summary>
        /// 构建分页查询
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="pageSize"></param>
        /// <param name="nowPage"></param>
        /// <returns></returns>
        string SelectPageList(string Table, int pageSize, int nowPage, string[] orderBy = null,string orderType = "asc");

        /// <summary>
        /// 带条件分页查询
        /// </summary>
        /// <param name="Table"></param>
        /// <param name="pageSize"></param>
        /// <param name="nowPage"></param>
        /// <param name="conditions">条件属性名</param>
        /// <returns></returns>
        string SelectPageListWithCondition(string Table, int pageSize, int nowPage, string[] conditions, string[] orderBy = null, string orderType = "asc");

        /// <summary>
        /// 按传入的属性名和排序类型进行排序
        /// </summary>
        /// <param name="orderType">排序类型：desc，asc</param>
        /// <param name="property">需要排序的属性</param>
        /// <returns></returns>
        string OrderByString(string orderType, string[] property);

        /// <summary>
        /// 根据C#表达式构造SQL语句
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="express"></param>
        /// <returns></returns>
        KeyValuePair<string,object>[] SelectMany<T>(out string sql, Expression<Func<T, bool>> express, string TableName);


        KeyValuePair<string, object>[]  SelectPageListWithCondition<T>(out string sql, string Table, int pageSize, int nowPage, Expression<Func<T, bool>> condition, string[] orderBy = null, string orderType = "asc");

        /// <summary>
        /// 查询最后一次插入的行记录，表主键为自增且只能有1个主键
        /// </summary>
        /// <param name="Table">表名</param>
        /// <param name="primaryKey">主键名</param>
        /// <returns></returns>
        string SelectLastInsertRow(string Table,string primaryKey);
    }
}
