using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyORM.Attributes;

namespace MyORM
{
    public enum DataBaseTypesEnum
    {   
        /// <summary>
        /// 默认值，未设置，直接使用会冒异常
        /// </summary>
        [EnumHelper("Unknow")]
        Unknow=0,

        /// <summary>
        /// MySQL数据库
        /// </summary>
        [EnumHelper("MySQLService")]
        MySQL=1,

        /// <summary>
        /// SQL Server数据库
        /// </summary>
        [EnumHelper("SQLServerService")]
        SQLServer=2,

        /// <summary>
        /// SQLite数据库
        /// </summary>
       [EnumHelper("SQLiteService")]
       SQLite = 3
    }

    
}
