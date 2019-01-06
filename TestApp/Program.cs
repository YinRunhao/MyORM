using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyORM;
using MyORM.DbService;
using System.Linq.Expressions;
using System.Collections;
using System.Data.SQLite;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string exePath = @"F:\C_shap\Git\MyORM\branches\SQL-Inject-defence\TestApp";
            string mySQL_conStr = "Your MySQL connection string";
            string sqlServer_conStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=" + exePath + @"\TestDataBases\SQLServerDB.mdf" + ";Integrated Security=True;Connect Timeout=30";
            string sqlite_conStr = @"Data Source=" + exePath + @"\TestDataBases\SQLiteDB.db";

            // step 1  SetConnectionString
            MySQLService.SetConnection(mySQL_conStr);
            SQLServerService.SetConnection(sqlServer_conStr);
            SQLiteService.SetConnection(sqlite_conStr);

            DataBaseTypesEnum DBType = DataBaseTypesEnum.SQLite;
            SQLService service = null;

            switch (DBType)
            {
                case DataBaseTypesEnum.MySQL:
                    {
                        //step 2 tell Service that which type of database you want to use in default
                        ModelBase.SetDefaultService(DataBaseTypesEnum.MySQL);

                        //step 3 create a new instance and use it
                        service = new MySQLService();
                        break;
                    }
                case DataBaseTypesEnum.SQLServer:
                    {
                        ModelBase.SetDefaultService(DataBaseTypesEnum.SQLServer);
                        service = new SQLServerService();
                        break;
                    }
                case DataBaseTypesEnum.SQLite:
                    {
                        ModelBase.SetDefaultService(DataBaseTypesEnum.SQLite);
                        service = new SQLiteService();
                        break;
                    }
            }

            LoadById(service);

            ReadAll(service);

            ReadByCondition(service);

            ReadPageList(service);

            Update(service);

            Insert(service);

            Delete(service);

            Console.ReadKey();
        }

        static void LoadById(SQLService service)
        {
            Console.WriteLine("-------------LoadByID-------------");
            int sid = 1;
            Student student = service.LoadByID<Student>(sid);
            PrintObj(student);
            Console.WriteLine("\n-------------LoadByID_End-------------\n");
        }

        static void ReadAll(SQLService service)
        {
            Console.WriteLine("-------------LoadAll-------------");
            List<Student> dataList = service.LoadAll<Student>();
            PrintObj(dataList);
            Console.WriteLine("-------------LoadAll_End-------------\n");
        }

        static void ReadByCondition(SQLService service)
        {
            int studentId = 2;
            string condition = "Mis";

            Console.WriteLine("-------------LoadMany_Lambda-------------");
            //using Lambda
            List<Learn> dataList = service.LoadMany<Learn>(s => s.Grade > 60 && s.StudentId == studentId);
            PrintObj(dataList);
            Console.WriteLine("-------------LoadMany_Lambda_End-------------\n");

            Console.WriteLine("-------------LoadMany_Contains-------------");
            //using Lambda include function Contains
            List<Teacher> teachers = service.LoadMany<Teacher>(s => s.Name.Contains(condition));
            PrintObj(teachers);
            Console.WriteLine("-------------LoadMany_Contains_End-------------\n");

            Console.WriteLine("-------------LoadMany_StartsWith-------------");
            //using Lambda include function StarWitch
            teachers = service.LoadMany<Teacher>(s => s.Name.StartsWith("Mr"));
            PrintObj(teachers);
            Console.WriteLine("-------------LoadMany_StartsWith_End-------------\n");

            Console.WriteLine("-------------LoadMany_EndWith-------------");
            //using Lambda include function EndWitch
            teachers = service.LoadMany<Teacher>(s => s.Name.EndsWith("Li"));
            PrintObj(teachers);
            Console.WriteLine("-------------LoadMany_EndWith_End-------------\n");

            Console.WriteLine("-------------LoadMany_Equals-------------");
            //using Lambda include function Equal
            teachers = service.LoadMany<Teacher>(s => s.Name.Equals("Mis.Swift"));
            PrintObj(teachers);
            Console.WriteLine("-------------LoadMany_Equals_End-------------\n");

            Console.WriteLine("-------------LoadByCondition-------------");
            //using Key Value pair
            KeyValuePair<string, object>[] keyValue = new KeyValuePair<string, object>[2]
                { new KeyValuePair<string, object>("StudentId",1),new KeyValuePair<string, object>("CourseId",2)};
            dataList = service.LoadByCondition<Learn>(keyValue);
            PrintObj(dataList);
            Console.WriteLine("-------------LoadByCondition_End-------------\n");

        }

        static void ReadPageList(SQLService service)
        {
            int count = 0;
            int pageSize = 1;
            int pageIdx = 1;

            Console.WriteLine("-------------LoadPageList_Lambda-------------");
            //using Lamdba Expression
            List<Learn> dataList = service.LoadPageList<Learn>(pageSize, pageIdx, ref count, s => s.Grade > 60, s => s.Grade, "desc");
            PrintObj(dataList);
            Console.WriteLine("-------------LoadPageList_Lambda_End-------------\n");

            Console.WriteLine("-------------LoadPageList_KeyValuePair-------------");
            //using KeyValuePair
            KeyValuePair<string, object>[] keyValue = new KeyValuePair<string, object>[1] { new KeyValuePair<string, object>("Grade", 100) };
            dataList = service.LoadPageList<Learn>(pageSize, pageIdx, ref count, keyValue);
            PrintObj(dataList);
            Console.WriteLine("-------------LoadPageList_KeyValuePair_End-------------\n");
        }

        static void Update(SQLService service)
        {
            Console.WriteLine("-------------UpdateModel-------------");
            //if this id is no data you can change another
            int TestId = 5;
            Learn learn = service.LoadByID<Learn>(TestId);
            if (learn != null)
            {
                Console.WriteLine("Origin Data:");
                PrintObj(learn);

                if (learn.Grade >= 100)
                {
                    learn.Grade = 60;
                }
                else
                {
                    learn.Grade = learn.Grade + 1;
                    service.UpdateModel(learn);
                }

                learn = service.LoadByID<Learn>(TestId);
                Console.WriteLine("\nData Updated:");
                if (learn != null)
                {
                    PrintObj(learn);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Get Data fail,this id may be not data");
                }
            }
            else
            {
                Console.WriteLine("Get Data fail,this id may be not data");
            }
            Console.WriteLine("-------------UpdateModel_End-------------\n");
        }

        static void Insert(SQLService service)
        {
            Console.WriteLine("-------------Add-------------");
            int beforeCount = 0;
            int afterCount = 0;

            Student newStudent = new Student();
            newStudent.Name = "Mike_InsertTest";
            newStudent.Birthday = DateTime.Now.Date;
            beforeCount = service.LoadMany<Student>(s => s.Name.Contains("InsertTest")).Count;

            if (service.Add(newStudent))
            {
                afterCount = service.LoadMany<Student>(s => s.Name.Contains("InsertTest")).Count;
                if (afterCount > beforeCount)
                {
                    Console.WriteLine("Insert success!");
                }
            }
            else
            {
                Console.WriteLine("Insert fail!");
            }
            Console.WriteLine("-------------Add_End-------------\n");
        }

        static void Delete(SQLService service)
        {
            Console.WriteLine("-------------DeleteModel-------------");
            Student student = service.LoadMany<Student>(s => s.Name.Contains("InsertTest")).FirstOrDefault();
            if (student != null)
            {
                if (service.DeleteModel(student))
                {
                    if (service.LoadByID<Student>(student.Sid) == null)
                    {
                        Console.WriteLine("Delete Success!");
                        Console.WriteLine("-------------DeleteModel_End-------------\n");
                        return;
                    }
                }
                Console.WriteLine("Delete Fail!");
            }
            Console.WriteLine("Delete Fail! Data not exist!");
            Console.WriteLine("-------------DeleteModel_End-------------\n");

        }

        private static void PrintObj(object obj)
        {

            Type type = obj.GetType();
            var collectionType = type.GetInterface("ICollection");
            if (collectionType != null)
            {
                var objlsit = obj as ICollection;
                foreach (var o in objlsit)
                {
                    PrintObj(o);
                    Console.WriteLine();
                }
            }
            else
            {
                var props = type.GetProperties();
                Console.Write("{");
                foreach (var p in props)
                {
                    if (p.PropertyType.Namespace == "System")
                    {
                        Console.Write(p.Name + ":");
                        if (null == p.GetValue(obj))
                        {
                            Console.Write("null");
                        }
                        else
                        {
                            Console.Write(p.GetValue(obj).ToString().TrimEnd() + ",");
                        }
                    }
                }
                Console.Write("}");
            }
        }
    }
}
