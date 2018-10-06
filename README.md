# MyORM
A light ORM tool which can using different type database at the same time, supporting MySQL, SQLServer, SQLite
This is a lightweight ORM tool that written by myself, which can dynamically change the database connection of the program at run time, so that the program can operate different databases at the same time.

It is used in the following steps:
0.Design Database

1.Setting Attributes for Entity class
  These Attributes are written by myself, they are packaging in MyORM.Attributes
  
  Example:
    [MyTable(Name ="Student")]
    public class Student : ModelBase
    {
        [MyPrimaryKey]
        [MyAutoIncrement]
        public int Sid { get; set; }
        public string Name { get; set; }
        public DateTime Birthday { get; set; }
        [MyMappingList(TableName = "Learn", ForeignKeys = new string[1] { "StudentId" })]
        public ICollection<Learn> learns { get { return this.MappingListInit<Learn>(); } }
    }
    
2.set connecting string for different type database

     MySQLService.SetConnection(mySQL_conStr);
     SQLServerService.SetConnection(sqlServer_conStr);
     SQLiteService.SetConnection(sqlite_conStr);

3.Select the type of database you want to use now

    ModelBase.SetService(DataBaseTypesEnum.SQLite);
    
4.Instantiate the Service object and use it

     SQLService service = new SQLiteService();
     List<Student> stuList = service.LoadAll<Student>();
     
if you want to change other type database call ModelBase.SetService(DataBaseTypesEnum);
