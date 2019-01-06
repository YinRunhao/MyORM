using MyORM.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;
using MyORM.ExpressionTools;
using MyORM.DbHelper;
using MyORM.DbStringBuilder;


namespace MyORM.DbService
{
    public abstract class SQLService
    {
        protected ISQLHelper helper;

        protected ISQLStringBuilder stringBuilder;

        protected abstract void OpenConnection();

        /// <summary>
        /// 将对象转换为一条数据库记录并存入
        /// </summary>
        /// <param name="model">要存入的对象</param>
        /// <returns></returns>
        public virtual bool Add(ModelBase model)
        {
            OpenConnection();
            Type t = model.GetType();
            var props = t.GetProperties();
            List<PropertyInfo> baseProps = new List<PropertyInfo>();
            PropertyInfo primaryKey = null;
            int primaryKeyCnt = 0;

            foreach (var p in props)
            {
                if (p.PropertyType.Namespace == "System")
                {
                    baseProps.Add(p);
                }
            }

            KeyValuePair<string, object>[] vals = new KeyValuePair<string, object>[baseProps.Count];
            string[] propNms = new string[baseProps.Count];
            int i = 0;
            foreach (var p in baseProps)
            {
                string key = p.Name;
                object value = null;
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                {
                    primaryKeyCnt++;
                }
                if (p.IsDefined(typeof(MyAutoIncrementAttribute)) && Convert.ToInt32(p.GetValue(model)) == 0)
                {
                    continue;
                }
                value = p.GetValue(model);
                vals[i] = new KeyValuePair<string, object>(key, value);
                propNms[i] = key;
                i++;
            }
            string sql = stringBuilder.InsertString(GetTableName(t), propNms);
            int res = helper.DoUpdate(sql,vals);

            // UpdateModel
            if (1 == primaryKeyCnt && 0 < res)
            {
                primaryKey = FindAutoIncrementPrimaryKey(t);
                if (null != primaryKey)
                {
                    sql = stringBuilder.SelectLastInsertRow(GetTableName(t), primaryKey.Name);
                    var table = helper.DoSelect(sql);
                    initModel(table.Rows[0], t, model);
                }
            }

            helper.ShutDown();
            return res > 0;
        }

        /// <summary>
        /// 删除某个对象在数据库中的记录（根据表的主键进行查找）
        /// </summary>
        /// <param name="model">要删除的对象</param>
        /// <returns></returns>
        public virtual bool DeleteModel(ModelBase model)
        {
            OpenConnection();
            Type t = model.GetType();
            var props = t.GetProperties();
            List<PropertyInfo> primaryKeys = new List<PropertyInfo>();
            foreach (var p in props)
            {
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                {
                    primaryKeys.Add(p);
                }
            }

            KeyValuePair<string, object>[] primary = new KeyValuePair<string, object>[primaryKeys.Count];
            string[] propNms = new string[primaryKeys.Count];
            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string key = primaryKeys[i].Name;
                var val = primaryKeys[i].GetValue(model);
                primary[i] = new KeyValuePair<string, object>(key, val);
                propNms[i] = key;
            }

            string sql = stringBuilder.DeleteString(GetTableName(t), propNms);
            int result = helper.DoUpdate(sql, primary);
            helper.ShutDown();
            return result > 0;
        }

        /// <summary>
        /// 加载某个表的所有记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> LoadAll<T>() where T : ModelBase
        {
            OpenConnection();
            List<T> result = new List<T>();
            string sql = stringBuilder.SelectAllString<T>();
            DataTable table = helper.DoSelect(sql);
            initObjectList<T>(result, table);
            helper.ShutDown();
            return result;
        }

        /// <summary>
        /// 根据传入的条件对表查询并返回对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conditions"></param>
        /// <returns></returns>
        public virtual List<T> LoadByCondition<T>(KeyValuePair<string, object>[] conditions, Expression<Func<T, object>> orderBy = null, string orderType = "asc") where T : ModelBase
        {
            List<T> result = new List<T>();
            Type t = typeof(T);
            List<string> orders = new List<string>();

            if (orderBy != null)  //有传排序属性就按排序属性排序
            {
                orders.Add(ExpressionHandle.DealGetPropertyNameExpression<T>(orderBy));
            }
            else    //否则按主键排序
            {
                var props = t.GetProperties();
                foreach (var p in props)
                {
                    if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                    {
                        orders.Add(p.Name);
                    }
                }
            }

            // 获取作为条件的属性的属性名
            string[] propNms = GetKeysArray(conditions);

            string[] order = orders.ToArray();
            string sql = stringBuilder.SelectByCondition(GetTableName(t), propNms, order, orderType);
            OpenConnection();
            var table = helper.DoSelect(sql,conditions);
            initObjectList<T>(result, table);
            helper.ShutDown();
            return result;
        }

        /// <summary>
        /// 根据主键进行查询,查不到返回NULL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        public virtual T LoadByID<T>(params object[] primaryKeys) where T : ModelBase
        {
            Type t = typeof(T);
            var props = t.GetProperties();
            List<PropertyInfo> primarykeys = new List<PropertyInfo>();
            foreach (var p in props)
            {
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute), false))
                {
                    primarykeys.Add(p);
                }
            }
            KeyValuePair<string, object>[] conditions = new KeyValuePair<string, object>[primarykeys.Count];
            int i = 0;
            foreach (var p in primarykeys)
            {
                conditions[i] = new KeyValuePair<string, object>(p.Name, primaryKeys[i]);
            }

            return LoadByID<T>(conditions);
        }

        /// <summary>
        /// 根据C#表达式对表进行查询（暂时支持部分表达式，若不支持会冒出ArgumentException）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="express"></param>
        /// <returns></returns>
        public virtual List<T> LoadMany<T>(Expression<Func<T, bool>> express) where T : ModelBase
        {
            OpenConnection();
            string sql = "";
            var conditions = stringBuilder.SelectMany(out sql,express, GetTableName(typeof(T)));
            List<T> result = new List<T>();
            var table = helper.DoSelect(sql, conditions);
            initObjectList<T>(result, table);
            helper.ShutDown();
            return result;
        }


        /// <summary>
        /// 对表进行分页查询并返回对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="nowPage">目前在第几页</param>
        /// <param name="infoCount">记录总数</param>
        /// <param name="condition">查询条件表达式</param>
        /// <param name="orderBy">排序</param>
        /// <param name="orderType">排序类型:asc,desc</param>
        /// <returns></returns>
        public virtual List<T> LoadPageList<T>(int pageSize, int nowPage, ref int infoCount, Expression<Func<T, bool>> condition = null, Expression<Func<T, object>> orderBy = null, string orderType = "asc") where T : ModelBase
        {
            var result = new List<T>();
            DataTable table = null;
            OpenConnection();
            var tragetType = typeof(T);
            List<string> orders = new List<string>();
            if (orderBy != null)  //有传排序属性就按排序属性排序
            {
                orders.Add(ExpressionHandle.DealGetPropertyNameExpression<T>(orderBy));
            }
            else    //否则按主键排序
            {
                Type t = typeof(T);
                var props = t.GetProperties();
                foreach (var p in props)
                {
                    if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                    {
                        orders.Add(p.Name);
                    }
                }
            }
            string[] order = orders.ToArray();
            string sql = "";// BuildPageSelectSql();

            if (condition == null)
            {
                sql = stringBuilder.SelectPageList(GetTableName(tragetType), pageSize, nowPage, order, orderType);
                table = helper.DoSelect(sql);
            }
            else
            {
                var conditions = stringBuilder.SelectPageListWithCondition(out sql,GetTableName(tragetType),pageSize,nowPage,condition, order, orderType);
                table = helper.DoSelect(sql,conditions);
            }
            initObjectList<T>(result, table);
            if (table.Rows.Count > 0)
            {
                infoCount = Convert.ToInt32(table.Rows[0]["cnt"]);
            }
            helper.ShutDown();
            return result;
        }

        /// <summary>
        /// 对表进行分页查询并返回对象集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pageSize">每页记录数</param>
        /// <param name="nowPage">目前在第几页</param>
        /// <param name="infoCount">记录总数</param>
        /// <param name="condition">查询条件</param>
        /// <param name="orderBy">排序</param>
        /// <param name="orderType">排序类型:asc,desc</param>
        /// <returns></returns>
        public virtual List<T> LoadPageList<T>(int pageSize, int nowPage, ref int infoCount, KeyValuePair<string, object>[] conditions = null, Expression<Func<T, object>> orderBy = null, string orderType = "asc") where T : ModelBase
        {
            var result = new List<T>();
            OpenConnection();
            var tragetType = typeof(T);
            List<string> orders = new List<string>();
            if (orderBy != null)  //有传排序属性就按排序属性排序
            {
                orders.Add(ExpressionHandle.DealGetPropertyNameExpression<T>(orderBy));
            }
            else    //否则按主键排序
            {
                Type t = typeof(T);
                var props = t.GetProperties();
                foreach (var p in props)
                {
                    if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                    {
                        orders.Add(p.Name);
                    }
                }
            }
            string[] order = orders.ToArray();
            string sql = "";// BuildPageSelectSql();
            string[] propNms = GetKeysArray(conditions);
            if (conditions == null)
                sql = stringBuilder.SelectPageList(tragetType.Name, pageSize, nowPage, order, orderType);
            else
                sql = stringBuilder.SelectPageListWithCondition(tragetType.Name, pageSize, nowPage, propNms, order, orderType);
            var table = helper.DoSelect(sql,conditions);
            initObjectList<T>(result, table);
            if (table.Rows.Count > 0)
            {
                infoCount = Convert.ToInt32(table.Rows[0]["cnt"]);
            }
            helper.ShutDown();
            return result;
        }

        /// <summary>
        /// 更新该对象在数据库中的记录
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public virtual bool UpdateModel(ModelBase model)
        {
            OpenConnection();
            Type t = model.GetType();
            var props = t.GetProperties();
            List<PropertyInfo> baseprop = new List<PropertyInfo>();
            List<PropertyInfo> primarykey = new List<PropertyInfo>();
            string[] primaryProps = null;       // 主键属性名数组
            string[] modifiedProp = null;       // 有改动的属性属性名数组
            List<KeyValuePair<string, object>> paramList = new List<KeyValuePair<string, object>>();
            foreach (var p in props)
            {
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                {
                    primarykey.Add(p);
                    continue;
                }
                if (p.PropertyType.Namespace == "System")
                {
                    baseprop.Add(p);
                }
            }
            int i = 0;
            KeyValuePair<string, object>[] primarykeys = new KeyValuePair<string, object>[primarykey.Count];
            i = 0;
            foreach (var p in primarykey)
            {
                string key = p.Name;
                object value = p.GetValue(model).ToString();
                primarykeys[i] = new KeyValuePair<string, object>(key, value);
                i++;
            }
            primaryProps = GetKeysArray(primarykeys);
            string sql = stringBuilder.SelectOneRowByID(GetTableName(t), primaryProps);
            DataTable table = helper.DoSelect(sql, primarykeys);
            var oldObj = Activator.CreateInstance(t);
            var oldModel = oldObj as ModelBase;
            initModel(table.Rows[0], t, oldModel);
            var values = FindDifference(oldModel, model);
            modifiedProp = GetKeysArray(values);
            if (values.Length == 0)     //No Changes
                return true;
            sql = stringBuilder.UpdateString(GetTableName(t), modifiedProp, primaryProps);
            paramList.AddRange(values);
            paramList.AddRange(primarykeys);
            int result = helper.DoUpdate(sql, paramList.ToArray());
            helper.ShutDown();
            return result > 0;
        }

        private KeyValuePair<string, object>[] FindDifference(ModelBase oldObj, ModelBase newObj)
        {
            List<KeyValuePair<string, object>> updateData = new List<KeyValuePair<string, object>>();
            if (oldObj.GetType() == newObj.GetType())
            {
                var type = oldObj.GetType();
                var props = type.GetProperties();
                foreach (var pro in props)
                {
                    if (pro.PropertyType.Namespace != "System")    //外键等对象属性不需要比较
                    {
                        continue;
                    }

                    object oldVal = pro.GetValue(oldObj);
                    object newVal = pro.GetValue(newObj);

                    if (null == oldVal && null == newVal)
                    {
                        continue;
                    }
                    // always update
                    if (pro.PropertyType.IsArray)
                    {
                        updateData.Add(new KeyValuePair<string, object>(pro.Name, newVal));
                    }
                    else
                    {
                        if (!oldVal.Equals(newVal))
                        {
                            updateData.Add(new KeyValuePair<string, object>(pro.Name, newVal));
                        }
                    }
                }
                return updateData.ToArray();
            }
            else
            {
                throw new ArgumentException("传入的两个对象类型不一样");
            }
        }

        /// <summary>
        /// 根据主键进行查询,查不到返回NULL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="primaryKeys"></param>
        /// <returns></returns>
        public virtual T LoadByID<T>(params KeyValuePair<string, object>[] primaryKeys) where T:ModelBase
        {
            Type t = typeof(T);
            string[] propNms = GetKeysArray(primaryKeys);
            string sql = stringBuilder.SelectOneRowByID(GetTableName(t), propNms);
            OpenConnection();
            var table = helper.DoSelect(sql, primaryKeys);
            if (0 == table.Rows.Count)
            {
                helper.ShutDown();
                return null;
            }
            else
            {
                T result = Activator.CreateInstance<T>();
                initModel<T>(table.Rows[0], result);
                helper.ShutDown();
                return result;
            }
        }

        /// <summary>
        /// 从DataTable中初始化每一行为一个对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="table"></param>
        private void initObjectList<T>(List<T> result, DataTable table) where T : ModelBase
        {
            foreach (DataRow dr in table.Rows)
            {
                T newobj = (T)Activator.CreateInstance(typeof(T));
                initModel<T>(dr, newobj);
                result.Add(newobj);
            }
        }

        private string GetTableName(Type type)
        {
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
            return name;
        }

        /// <summary>
        /// 根据数据行初始化对象
        /// </summary>
        /// <param name="dr"></param>
        /// <param name="t"></param>
        private void initModel(DataRow dr, Type t,ModelBase model)
        {
            if (t.BaseType != typeof(ModelBase))
            {
                throw new ArgumentException("要操作类型必须继承ModelBase");
            }
            else
            {
                PropertyInfo property = null;
                var cols = dr.Table.Columns;
                string propNm = "";
                foreach (DataColumn col in cols)
                {
                    propNm = col.ColumnName;
                    property = t.GetProperty(propNm);
                    if (null != property)
                    {
                        if (dr[col] is DBNull)
                        {
                            property.SetValue(model, null);
                            continue;
                        }
                        //Sqlite以string类型存储时间，C#不能默认转换
                        if (col.DataType == typeof(DateTime))
                        {
                            DateTime dt = Convert.ToDateTime(dr[col]);
                            property.SetValue(model, dt);
                            continue;
                        }
                        // sqlite中integer用INT64存储
                        if (col.DataType == typeof(Int64) && property.PropertyType == typeof(Int32))
                        {
                            property.SetValue(model, Convert.ToInt32(dr[col]));
                            continue;
                        }
                        property.SetValue(model, dr[col]);
                    }
                }
            }
        }

        /// <summary>
        /// 根据数据行初始化对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dr"></param>
        private void initModel<T>(DataRow dr, ModelBase model) where T : ModelBase
        {
            var t = model.GetType();
            initModel(dr, t,model);
        }

        /// <summary>
        /// 查找传入类型中既是主键又是自增的属性
        /// </summary>
        /// <param name="modelType"></param>
        /// <returns></returns>
        private PropertyInfo FindAutoIncrementPrimaryKey(Type modelType)
        {
            var propList = modelType.GetProperties();
            PropertyInfo ret = null;
            foreach (PropertyInfo item in propList)
            {
                if (item.IsDefined(typeof(MyAutoIncrementAttribute)))
                {
                    if (item.IsDefined(typeof(MyPrimaryKeyAttribute)))
                    {
                        ret = item;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 获取KeyValuePair中Key值组成的数组
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        private string[] GetKeysArray(KeyValuePair<string, object>[] keyValues)
        {
            string[] ret = new string[keyValues.Length];
            for (int i = 0; i < keyValues.Length; i++)
            {
                ret[i] = keyValues[i].Key;
            }
            return ret;
        }
        /*private bool ByteArrCompare(Byte[] arr1,Byte[] arr2)
        {
            bool ret = true;
            if (null == arr1 || null == arr2)
            {
                ret = false;
            }
            else if (arr1.Length == arr2.Length)
            {
                for (int i = 0; i < arr2.Length; i++)
                {
                    if (arr1[i] != arr2[i])
                    {
                        ret = false;
                    }
                }
            }
            else
            {
                ret = false;
            }
            return ret;
        }*/
    }
}
