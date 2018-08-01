using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ContactLibrary
{
    [DataContract] // want to serialize the directory class
    public class ContactDirectory
    {
        [DataMember] // the only data member that needs to be serialize
        public ICollection<Person> People { get; set; } // can be any generic collection, I am using list
        // other data members
        public DbHandler db = null; // for handling sql objects
        public NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger(); // for logging exceptions
        public static readonly string PrettyFormat = "{0,4} {1,15} {2,15} {3,8} {4,20} {5,20} {6,20} {7,20} {8,7} {9,15} {10,15} {11,12} {12,5}";

        public ContactDirectory()
        {
            People = new List<Person>();
            db = new DbHandler();
        }

        public ContactDirectory(ContactDirectory cd)
        {
            People = new List<Person>();
            foreach (Person p in cd.People)
            {
                People.Add(p);
            }
            db = new DbHandler();
        }

        public void AddPerson(Person p)
        {
            People.Add(p);
        }

        public void AddPersonToDatabase(Person p)
        {
            try
            {
                db.cmd.CommandText = $"insert into dbo.Person (Pid, FirstName, LastName) values ({p.Pid}, '{p.FirstName}', '{p.LastName}') "
                    + "insert into dbo.Address (Pid, HouseNum, Street, City, State, Country, ZipCode) values "
                    + $"({p.Address.Pid}, '{p.Address.HouseNum}', '{p.Address.Street}', '{p.Address.City}', '{p.Address.State}', '{p.Address.Country}', '{p.Address.ZipCode}') "
                    + $"insert into dbo.Phone (Pid, CountryCode, AreaCode, PhoneNumber, Extension) values "
                    + $"({p.Phone.Pid}, '{p.Phone.CountryCode}', '{p.Phone.AreaCode}', '{p.Phone.Number}', '{p.Phone.Ext}')";
                db.cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while inserting values.");
            }
        }

        public Person ReadPerson(long Pid)
        {
            IEnumerable<Person> query =
                from person in People
                where person.Pid.Equals(Pid)
                select person;
            if (!query.Any())
            {
                Console.WriteLine($"Person with Pid {Pid} does not exist!");
                return null;
            }
            return query.Single();
        }

        public string ReadPersonFromDatabase(long Pid)
        {
            db.cmd.CommandText = "select per.Pid, FirstName, LastName, HouseNum, Street, City, State, Country, ZipCode, CountryCode, AreaCode, PhoneNumber, Extension " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid inner join dbo.Phone as pho on per.Pid = pho.Pid " +
                $"where per.Pid = {Pid}";
            db.reader = db.cmd.ExecuteReader();
            string result = "";
            if (db.reader.Read())
            {
                result = string.Format(PrettyFormat,
                            db.reader[0], db.reader[1], db.reader[2], db.reader[3], db.reader[4], db.reader[5], db.reader[6], db.reader[7], db.reader[8], db.reader[9], db.reader[10], db.reader[11], db.reader[12]);
            }
            else
            {
                Console.WriteLine($"Person with Pid {Pid} does not exist!");
            }
            db.reader.Close();
            return result;
        }

        public void DeletePerson(long Pid)
        {
            Person p = ReadPerson(Pid);
            if (p != null)
                People.Remove(p);
        }

        public void DeletePersonFromDatabase(long Pid)
        {
            db.cmd.CommandText = $"delete from dbo.Person where Pid = {Pid}";
            try
            {
                db.cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while deleting the entry.");
            }
        }

        private readonly Dictionary<int, string> PropertyMap = new Dictionary<int, string>()
        { { 0, "Id" }, { 1, "FirstName" }, { 2, "LastName" }, { 3, "HouseNum" }, { 4, "Street" },
            { 5, "City" }, { 6, "State" }, { 7, "Country" }, { 8, "ZipCode" }, { 9, "CountryCode" },
            { 10, "AreaCode" }, { 11, "Number" }, { 12, "Ext" } };

        public void UpdatePerson(long Pid, int property, string val)
        {
            Person p = ReadPerson(Pid);
            if (p != null)
            {
                switch (property)
                {
                    case int n when n <= 2:
                        p.GetType().GetProperty(PropertyMap[property]).SetValue(p, val);
                        break;
                    case int n when n <= 8:
                        p.Address.GetType().GetProperty(PropertyMap[property]).SetValue(p.Address, val);
                        break;
                    case int n when n <= 12:
                        p.Phone.GetType().GetProperty(PropertyMap[property]).SetValue(p.Phone, val);
                        break;
                    case int n when n < 0 || n > 12:
                        Console.WriteLine("Invalid choice selected. No such property.");
                        break;
                }
            }
        }

        public void UpdatePersonInDatabase(long Pid, int property, string val)
        {
            switch (property)
            {
                case int n when n <= 2:
                    db.cmd.CommandText = "update dbo.Person";
                    break;
                case int n when n <= 8:
                    db.cmd.CommandText = "update dbo.Address";
                    break;
                case int n when n <= 12:
                    db.cmd.CommandText = "update dbo.Phone";
                    break;
                case int n when n <= 0 || n > 12:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    break;
            }
            db.cmd.CommandText += $" set {PropertyMap[property]} = '{val}' where Pid = {Pid}";
            try
            {
                db.cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while updating the entry.");
            }
        }

        public List<Person> SearchPerson(int property, string val)
        {
            IEnumerable<Person> query;
            switch (property)
            {
                case 1:
                    query =
                        from person in People
                        where person.FirstName.Contains(val)
                        select person;
                    break;
                case 2:
                    query =
                        from person in People
                        where person.LastName.Contains(val)
                        select person;
                    break;
                case 3:
                    query =
                        from person in People
                        where person.Address.ZipCode.Contains(val)
                        select person;
                    break;
                case 4:
                    query =
                        from person in People
                        where person.Address.City.Contains(val)
                        select person;
                    break;
                case 5:
                    query =
                        from person in People
                        let phoneNumber = person.Phone.CountryCode +
                            person.Phone.AreaCode +
                            person.Phone.Number
                        where phoneNumber.Contains(val)
                        select person;
                    break;
                default:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    return null;
            }
            return query.ToList();
        }

        public string SearchPersonInDatabase(int property, string val)
        {
            db.cmd.CommandText = "select per.Pid, FirstName, LastName, " +
                "HouseNum, Street, City, State, Country, ZipCode, " +
                "CountryCode, AreaCode, PhoneNumber, Extension " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid " +
                "inner join dbo.Phone as pho on per.Pid = pho.Pid ";
            switch (property)
            {
                case 1: // first name
                case 2: // last name
                    db.cmd.CommandText += $"where {PropertyMap[property]} like '%{val}%'";
                    break;
                case 3: // zip code
                    property = 8;
                    db.cmd.CommandText += $"where {PropertyMap[property]} like '%{val}%'";
                    break;
                case 4: // city
                    property = 5;
                    db.cmd.CommandText += $"where {PropertyMap[property]} like '%{val}%'";
                    break;
                case 5: // full phone number
                    db.cmd.CommandText += $"where concat(CountryCode, AreaCode, PhoneNumber) like '%{val}%'";
                    break;
                default:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    return null;
            }
            string result = "";
            db.reader = db.cmd.ExecuteReader();
            while (db.reader.Read())
            {
                result += string.Format(PrettyFormat,
                            db.reader[0], db.reader[1], db.reader[2], db.reader[3], db.reader[4], db.reader[5], db.reader[6], db.reader[7], db.reader[8], db.reader[9], db.reader[10], db.reader[11], db.reader[12]);
                result += Environment.NewLine;
            }
            db.reader.Close();
            return result.TrimEnd('\t', '\n', '\r', ' ');
        }

        public void ShowAllPerson()
        {
            PrintLegend();
            foreach (Person p in People)
            {
                Console.WriteLine(p.ToString());
            }
        }

        public void PrintLegend()
        {
            Console.WriteLine(PrettyFormat,
                            "Pid", "First Name", "Last Name", "House #", "Street", "City", "State", "Country", "Zip", "Country Code", "Area Code", "Phone #", "Ext");
        }

        public void LoadDatabaseEntries()
        {
            db.cmd.CommandText = "select per.Pid, FirstName, LastName, HouseNum, Street, City, State, Country, ZipCode, CountryCode, AreaCode, PhoneNumber, Extension " +
                    "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid inner join dbo.Phone as pho on per.Pid = pho.Pid " +
                    "order by per.Pid";
            db.reader = db.cmd.ExecuteReader();
            while (db.reader.Read())
            {
                Person p = new Person();
                p.Pid = p.Address.Pid = p.Phone.Pid = (long)db.reader[0];
                p.FirstName = (string)db.reader[1];
                p.LastName = (string)db.reader[2];
                p.Address.HouseNum = (string)db.reader[3];
                p.Address.Street = (string)db.reader[4];
                p.Address.City = (string)db.reader[5];
                p.Address.State = (string)db.reader[6];
                p.Address.Country = (string)db.reader[7];
                p.Address.ZipCode = (string)db.reader[8];
                p.Phone.CountryCode = (string)db.reader[9];
                p.Phone.AreaCode = (string)db.reader[10];
                p.Phone.Number = (string)db.reader[11];
                p.Phone.Ext = (string)db.reader[12];
                AddPerson(p);
            }
            db.reader.Close();
        }

        public void ClearDatabaseEntries() // for deleting every entry in database
        {
            db.cmd.CommandText = "delete dbo.Person delete dbo.Address delete dbo.Phone";
            try
            {
                db.cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error clearing database entries.");
            }
        }

        public bool ValidateID(long Pid) // to make sure id doesn't repeat when adding a person
        {
            IEnumerable<Person> query =
                from person in People
                where person.Pid.Equals(Pid)
                select person;
            if (!query.Any())
                return true;
            Console.WriteLine($"Person with Pid {Pid} already exists!");
            return false;
        }
    }

    public class DbHandler
    {
        public SqlConnection con = null;
        public SqlCommand cmd = null;
        public SqlDataReader reader = null;

        public DbHandler()
        {
            con = new SqlConnection("Data Source=chancunysps.database.windows.net;Initial Catalog=TestDb;Persist Security Error=True;User ID=cyy5113;Password=Password12345");
            con.Open();
            cmd = new SqlCommand("", con);
        }
    }

    public class JsonHelper
    {
        public static string JsonSerializer<T>(T t)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream();
            ser.WriteObject(ms, t);
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            ms.Close();
            return jsonString;
        }
        public static T JsonDeserializer<T>(string jsonString)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString));
            T obj = (T)ser.ReadObject(ms);
            return obj;
        }
    }

    public class MyLogger
    {
        public static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
    }
}
