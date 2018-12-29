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

            KeyValuePair<string, string>[] vals = new KeyValuePair<string, string>[baseProps.Count];
            int i = 0;
            foreach (var p in baseProps)
            {
                string key = p.Name;
                string value = "";
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                {
                    primaryKeyCnt++;
                }

                if (p.GetValue(model) == null)
                    value = "null";
                //AutoIncrement的Int类型未设定值的都默认为null
                else if (p.IsDefined(typeof(MyAutoIncrementAttribute)) && Convert.ToInt64(p.GetValue(model)) == 0)
                {
                    continue;
                }
                else if (p.PropertyType == typeof(DateTime))
                {
                    value = ((DateTime)p.GetValue(model)).ToString("s");
                }
                else
                    value = p.GetValue(model).ToString();
                vals[i] = new KeyValuePair<string, string>(key, value);
                i++;
            }
            string sql = stringBuilder.InsertString(GetTableName(t), vals);
            int res = helper.DoUpdate(sql);

            // UpdateModel
            if(1 == primaryKeyCnt && 0 < res)
            {
                primaryKey = FindAutoIncrementPrimaryKey(t);
                if (null != primaryKey)
                {
                    sql = stringBuilder.SelectLastInsertRow(GetTableName(t),primaryKey.Name);
                    var table = helper.DoSelect(sql);
                    initModel(table.Rows[0],t,model);
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

            KeyValuePair<string, string>[] primary = new KeyValuePair<string, string>[primaryKeys.Count];
            for (int i = 0; i < primaryKeys.Count; i++)
            {
                string key = primaryKeys[i].Name;
                var val = primaryKeys[i].GetValue(model).ToString();
                primary[i] = new KeyValuePair<string, string>(key, val);
            }

            string sql = stringBuilder.DeleteString(GetTableName(t), primary);
            int result = helper.DoUpdate(sql);
            helper.ShutDown();
            return result > 0;
        }

        /// <summary>
        /// 加载某个表的所有记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public virtual List<T> LoadAll<T>() where T:ModelBase
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
        public virtual List<T> LoadByCondition<T>(KeyValuePair<string, string>[] conditions,Expression<Func<T,object>> orderBy=null,string orderType = "asc") where T:ModelBase
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
            string[] order = orders.ToArray();
            string sql = stringBuilder.SelectByCondition(GetTableName(t), conditions,order,orderType);
            OpenConnection();
            var table = helper.DoSelect(sql);
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
        public virtual T LoadByID<T>(params object[] primaryKeys) where T:ModelBase
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
            KeyValuePair<string, string>[] conditions = new KeyValuePair<string, string>[primarykeys.Count];
            int i = 0;
            foreach (var p in primarykeys)
            {
                conditions[i] = new KeyValuePair<string, string>(p.Name, primaryKeys[i].ToString());
            }

            return LoadByID<T>(conditions);
        }

        /// <summary>
        /// 根据C#表达式对表进行查询（暂时支持部分表达式，若不支持会冒出ArgumentException）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="express"></param>
        /// <returns></returns>
        public virtual List<T> LoadMany<T>(Expression<Func<T, bool>> express) where T:ModelBase
        {
            OpenConnection();
            string sql = stringBuilder.SelectMany(express,GetTableName(typeof(T)));
            List<T> result = new List<T>();
            var table = helper.DoSelect(sql);
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
        public virtual List<T> LoadPageList<T>(int pageSize, int nowPage, ref int infoCount, Expression<Func<T, bool>> condition = null, Expression<Func<T, object>> orderBy = null, string orderType = "asc")where T:ModelBase
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
            if (condition == null)
                sql = stringBuilder.SelectPageList(tragetType.Name, pageSize, nowPage, order, orderType);
            else
                sql = stringBuilder.SelectPageListWithCondition(tragetType.Name, pageSize, nowPage, condition, order, orderType);
            var table = helper.DoSelect(sql);
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
        public virtual List<T> LoadPageList<T>(int pageSize, int nowPage, ref int infoCount, KeyValuePair<string, string>[] conditions = null, Expression<Func<T, object>> orderBy = null, string orderType = "asc") where T : ModelBase
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
            if (conditions == null)
                sql = stringBuilder.SelectPageList(tragetType.Name, pageSize, nowPage, order, orderType);
            else
                sql = stringBuilder.SelectPageListWithCondition(tragetType.Name, pageSize, nowPage, conditions, order, orderType);
            var table = helper.DoSelect(sql);
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
            //  KeyValuePair<string, string>[] values = new KeyValuePair<string, string>[baseprop.Count];
            int i = 0;
            KeyValuePair<string, string>[] primarykeys = new KeyValuePair<string, string>[primarykey.Count];
            i = 0;
            foreach (var p in primarykey)
            {
                string key = p.Name;
                string value = p.GetValue(model).ToString();
                primarykeys[i] = new KeyValuePair<string, string>(key, value);
                i++;
            }
            //var oldModel = LoadByID<>(primarykeys);
            string sql = stringBuilder.SelectOneRowByID(GetTableName(t), primarykeys);
            DataTable table = helper.DoSelect(sql);
            var oldObj = Activator.CreateInstance(t);
            var oldModel = oldObj as ModelBase;
            initModel(table.Rows[0], t,oldModel);
            var values = FindDifference(oldModel, model);
            if (values.Length == 0)     //No Changes
                return true;
            sql = stringBuilder.UpdateString(GetTableName(t), values, primarykeys);
            int result = helper.DoUpdate(sql);
            helper.ShutDown();
            return result > 0;
        }

        private KeyValuePair<string, string>[] FindDifference(ModelBase oldObj, ModelBase newObj)
        {
            List<KeyValuePair<string, string>> updateData = new List<KeyValuePair<string, string>>();
            if (oldObj.GetType() == newObj.GetType())
            {
                var type = oldObj.GetType();
                var props = type.GetProperties();
                foreach (var pro in props)
                {
                    string old = ""; 
                    string newV = "";
                    //DateTime类型的ToString可能会转出带中文星期几所以要严格制定转换格式
                    if (pro.PropertyType == typeof(DateTime))
                    {
                        old = ((DateTime)pro.GetValue(oldObj)).ToString("yyyy/MM/dd HH:mm:ss");
                        newV = ((DateTime)pro.GetValue(newObj)).ToString("yyyy/MM/dd HH:mm:ss");
                    }
                    else if (pro.PropertyType.Namespace != "System")    //外键等对象属性不需要比较
                    {
                        continue;
                    }
                    else
                    {
                        old = pro.GetValue(oldObj).ToString();
                        newV = pro.GetValue(newObj).ToString();
                    }
                    if (old.CompareTo(newV) != 0)
                    {
                        updateData.Add(new KeyValuePair<string, string>(pro.Name, newV));
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
        public virtual T LoadByID<T>(params KeyValuePair<string, string>[] primaryKeys) where T:ModelBase
        {
            Type t = typeof(T);
            string sql = stringBuilder.SelectOneRowByID(GetTableName(t), primaryKeys);
            OpenConnection();
            var table = helper.DoSelect(sql);
            if (0 == table.Rows.Count)
            {
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
    }
}
