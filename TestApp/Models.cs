using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MyORM;
using System.Threading.Tasks;
using MyORM.Attributes;


namespace TestApp
{
    [MyTable(Name ="Student")]
    public class Student : ModelBase
    {
        [MyPrimaryKey]
        [MyAutoIncrement]
        public int Sid { get; set; }

        public string Name { get; set; }

        public DateTime Birthday { get; set; }

        public Byte[] TestCol { get; set; }

        [MyMappingList(TableName = "Learn", ForeignKeys = new string[1] { "StudentId" })]
        public ICollection<Learn> learns { get { return this.MappingListInit<Learn>(); } }
    }

    [MyTable(Name = "Teacher")]
    public class Teacher : ModelBase
    {
        [MyPrimaryKey]
        [MyAutoIncrement]
        public int Tid { get; set; }

        public string Name { get; set; }

        [MyMappingList(TableName = "Course",ForeignKeys = new string[1] { "TeacherId" })]
        public ICollection<Course> courses{ get { return this.MappingListInit<Course>(); } }
    }

    [MyTable(Name ="Course")]
    public class Course : ModelBase
    {
        [MyPrimaryKey]
        [MyAutoIncrement]
        public int Cid { get; set; }

        public string Name { get; set; }

        [MyForeignKeyProperty(TableName = "Teacher",ColumnName ="Tid")]
        public int TeacherId { get; set; }

        [MyForeignKey(TableName ="Teacher",KeyName =new string[1] { "TeacherId"})]
        public Teacher Teacher { get { return this.Foreigninit<Teacher>(); } }
    }

    [MyTable(Name = "Learn")]
    public class Learn : ModelBase
    {
        [MyPrimaryKey]
        [MyAutoIncrement]
        public int LearnId { get; set; }

        [MyForeignKeyProperty(TableName = "Student",ColumnName = "Sid")]
        public int StudentId { get; set; }

        [MyForeignKeyProperty(TableName = "Course", ColumnName = "Cid")]
        public int CourseId { get; set; }

        public int Grade { get; set; }

        [MyForeignKey(TableName = "Student",KeyName = new string[1] { "StudentId" })]
        public Student Student { get { return this.Foreigninit<Student>(); } }

        [MyForeignKey(TableName = "Course", KeyName = new string[1] { "CourseId" })]
        public Course Course{ get { return this.Foreigninit<Course>(); } }
    }
}
