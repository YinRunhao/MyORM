using System;

namespace MyORM.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class MyTableAttribute : Attribute
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// 外键对象特性，配置在外键对象上
    /// Foreign Key Attribute, setting on foreign key OBJECT
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MyForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// 来自哪个表
        /// it comes from which table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 这个外键它在本类中叫什么名字,有可能由2个或以上属性映射1条记录，所以这里用数组
        /// This foreign key is what it is called in this class, it is possible to map 1 records by 2 or more property, so here is an array
        /// </summary>
        public string[] KeyName { get; set; }
    }

    /// <summary>
    /// 一对多集合对象特性
    /// 1:n Collection Object Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MyMappingListAttribute : Attribute
    {
        /// <summary>
        /// 是哪个表的外键
        /// it comes from which table
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 本表中的主键们在外键表中叫什么名字，注意顺序要和本类主键定义顺序一样
        /// The primary keys in this table are named what in the foreign key table,
        /// </summary>
        public string[] ForeignKeys { get; set; }
    }

    /// <summary>
    /// 主键特性
    /// Primary Key Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MyPrimaryKeyAttribute : Attribute
    {

    }

    /// <summary>
    /// 自增属性特性
    /// Auto Increment Attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MyAutoIncrementAttribute : Attribute
    {

    }

    /// <summary>
    /// 外键属性标记，设置在非对象的属性中
    /// Foreign key Property tag, set in Non-object properties
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class MyForeignKeyPropertyAttribute : Attribute
    {
        /// <summary>
        /// 来自哪个表
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// 在自己的表中叫什么名字
        /// </summary>
        public string ColumnName { get; set; }
    }

    /// <summary>
    /// 枚举描述帮助特性
    /// Enum Helper
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class EnumHelperAttribute : Attribute
    {
        /// <summary>
        /// 枚举描述
        /// </summary>
        public string Describe { get; set; }

        public EnumHelperAttribute(string describe)
        {
            this.Describe = describe;
        }
    }
}
