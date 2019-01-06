using System;
using System.Collections.Generic;
using MyORM.Attributes;
using System.Reflection;
using System.Threading;
using MyORM.DbService;

namespace MyORM
{
    public class ModelBase
    {
        private static ThreadLocal<DataBaseTypesEnum> dataBaseType = new ThreadLocal<DataBaseTypesEnum>(() => DataBaseTypesEnum.Unknow);
        private static DataBaseTypesEnum defaultDbType = DataBaseTypesEnum.Unknow;

        /// <summary>
        /// Set the default Database Type in this application, all threads will auto use this setting unless call ChangeService()
        /// </summary>
        /// <param name="DbType"></param>
        public static void SetDefaultService(DataBaseTypesEnum DbType)
        {
            defaultDbType = DbType;
        }

        /// <summary>
        /// Change Database type in current thread,other threads have no effect
        /// </summary>
        /// <param name="DbType">Database Type</param>
        public static void ChangeService(DataBaseTypesEnum DbType)
        {
            dataBaseType.Value = DbType;
        }

        /// <summary>
        /// 根据指定的数据库类型创建一个Service供使用
        /// 即建即用，不与ModelBase的生命周期一致
        /// </summary>
        /// <returns></returns>
        private SQLService GetSQLService()
        {
            SQLService service = null;
            DataBaseTypesEnum DbType = DataBaseTypesEnum.Unknow;

            //根据枚举的附加属性获取对应的数据库类型服务并创建对象
            if (dataBaseType.Value != DataBaseTypesEnum.Unknow || defaultDbType != DataBaseTypesEnum.Unknow)
            {
                // if not set type in this thread then use the default setting
                if (dataBaseType.Value == DataBaseTypesEnum.Unknow)
                {
                    dataBaseType.Value = defaultDbType;
                }
                DbType = dataBaseType.Value;

                var fieldName = Enum.GetName(typeof(DataBaseTypesEnum), DbType);
                var enumAttr = typeof(DataBaseTypesEnum).GetField(fieldName).GetCustomAttribute(typeof(EnumHelperAttribute)) as EnumHelperAttribute;
                //var enumAttr = attrb as EnumHelperAttribute;
                string serviceName = "MyORM.DbService." + enumAttr.Describe;
                var serviceType = Type.GetType(serviceName);
                if (serviceType == null)
                    throw new ArgumentException("未创建或路径错误，导致无法获取到数据库服务类：" + serviceName);
                service = Activator.CreateInstance(serviceType) as SQLService;
            }
            else
            {
                throw new ArgumentException("未指定数据库类型!");
            }
            return service;
        }

        /// <summary>
        /// Get the Database Type on current thread,if not setting it will use the default setting
        /// </summary>
        /// <returns></returns>
        public static DataBaseTypesEnum GetDataBaseType()
        {
            // if not set type in this thread then use the default setting
            if (dataBaseType.Value == DataBaseTypesEnum.Unknow)
            {
                dataBaseType.Value = defaultDbType;
            }

            return dataBaseType.Value;
        }

        /// <summary>
        /// 对外键对象初始化
        /// 实质上就是在外键表中查找主键为XXX的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T Foreigninit<T>() where T : ModelBase
        {
            var t = this.GetType();
            var props = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            string table = "";
            string[] keyNames = null;
            Type forType = typeof(T);
            SQLService service = GetSQLService();
            //查找要实例化的目标属性
            foreach (var pro in props)
            {
                if (pro.IsDefined(typeof(MyForeignKeyAttribute), false))
                {
                    if (pro.PropertyType == forType)
                    {
                        var attr = (MyForeignKeyAttribute)pro.GetCustomAttribute(typeof(MyForeignKeyAttribute));
                        table = attr.TableName;
                        keyNames = attr.KeyName;
                        break;
                    }
                }
            }

            if (keyNames != null)
            {
                KeyValuePair<string, string>[] primarykeys = new KeyValuePair<string, string>[keyNames.Length];
                int i = 0;
                foreach (string name in keyNames)
                {
                    var key = t.GetProperty(name);
                    var val = key.GetValue(this);

                    var attr = (MyForeignKeyPropertyAttribute)key.GetCustomAttribute(typeof(MyForeignKeyPropertyAttribute));
                    string foreignColumnName = attr.ColumnName;

                    primarykeys[i] = new KeyValuePair<string, string>(foreignColumnName, val.ToString());
                    i++;
                }
                if (service == null)
                    throw new Exception("使用数据库服务前未确定数据库类型");
                return service.LoadByID<T>(primarykeys);
            }
            else
                return null;

        }

        /// <summary>
        /// 对1对多对象初始化，如老师下有多个学生
        /// 实质上就是在目标表中查找外键X等于本表中某主键的记录
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected ICollection<T> MappingListInit<T>() where T : ModelBase
        {
            var t = this.GetType();
            var props = t.GetProperties(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            List<object> primaryKVal = new List<object>();
            PropertyInfo refProp = null;
            SQLService service = GetSQLService();
            foreach (var p in props)
            {
                if (p.IsDefined(typeof(MyPrimaryKeyAttribute)))
                {
                    primaryKVal.Add(p.GetValue(this));
                }
                else if (p.IsDefined(typeof(MyMappingListAttribute)))
                {
                    if (p.PropertyType == typeof(ICollection<T>))
                    {
                        refProp = p;
                    }
                }
            }

            var refAttr = (MyMappingListAttribute)refProp.GetCustomAttribute(typeof(MyMappingListAttribute));
            var primaryKeyName = refAttr.ForeignKeys;

            KeyValuePair<string, object>[] conditions = new KeyValuePair<string, object>[primaryKVal.Count];
            for (int i = 0; i < primaryKVal.Count; i++)
            {
                conditions[i] = new KeyValuePair<string, object>(primaryKeyName[i], primaryKVal[i]);
            }
            if (service == null)
                throw new Exception("使用数据库服务前未确定数据库类型");
            return service.LoadByCondition<T>(conditions);
        }
    }
}
