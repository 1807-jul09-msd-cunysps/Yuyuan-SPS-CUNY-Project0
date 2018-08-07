using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using ContactDirectoryLib;

namespace DirectoryDAL
{
    public class DbHandler
    {
        public SqlConnection Con { get; private set; }
        public SqlCommand Cmd { get; private set; }
        public SqlDataReader Reader { get; private set; }
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger(); // for logging exceptions

        public DbHandler()
        {
            Con = new SqlConnection("Data Source=chancunysps.database.windows.net;Initial Catalog=TestDb;Persist Security Info=True;User ID=contactdirapp;Password=Password12345");
            Cmd = new SqlCommand("", Con);
        }

        private readonly Dictionary<int, string> DbParamMap = new Dictionary<int, string>()
        {
            { 0, "@Pid" }, { 1, "@FirstName" }, { 2, "@LastName" }, { 3, "@Age" }, { 4, "@Gender" }, { 5, "@HouseNum" },
            { 6, "@Street" }, { 7, "@City" }, { 8, "@State" }, { 9, "@Country" }, { 10, "@ZipCode" }, { 11, "@CountryCode" },
            { 12, "@Number" }, { 13, "@Ext" }, { 14, "@EmailAddress" }
        };

        public void AddPerson(Person p) // add with identity insert
        {
            Cmd.CommandText = "set identity_insert dbo.Person on " +
                "insert into dbo.Person (Pid, FirstName, LastName, Age, Gender) values (@Pid, @FirstName, @LastName, @Age, @Gender) " +
                "insert into dbo.Address values (@Pid, @HouseNum, @Street, @City, @State, @Country, @ZipCode) " +
                "insert into dbo.Phone values (@Pid, @CountryCode, @Number, @Ext) " +
                "insert into dbo.Email values (@Pid, @EmailAddress) " +
                "set identity_insert dbo.Person off";
            Cmd.Parameters.Clear();
            ArrayList data = new ArrayList
            {
                p.Pid, p.FirstName, p.LastName, p.Age, p.Gender.ToString(),
                p.Address.HouseNum, p.Address.Street, p.Address.City, p.Address.State, p.Address.Country, p.Address.ZipCode,
                p.Phone.CountryCode, p.Phone.Number, p.Phone.Ext, p.Email.EmailAddress
            };
            for (int i = 0; i < DbParamMap.Count; i++)
            {
                switch (i)
                {
                    case int n when n == 0 || n == 12:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (long)data[i]);
                        break;
                    case int n when n == 3 || n == 11:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (int)data[i]);
                        break;
                    case int n when n == 1 || n == 2 || (n >= 4 && n <= 10):
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (string)data[i]);
                        break;
                    case 13:
                        if ((int?)data[i] == null)
                            Cmd.Parameters.AddWithValue(DbParamMap[i], DBNull.Value);
                        else
                            Cmd.Parameters.AddWithValue(DbParamMap[i], (int)data[i]);
                        break;
                    case 14:
                        if ((string)data[i] == null)
                            Cmd.Parameters.AddWithValue(DbParamMap[i], DBNull.Value);
                        else
                            Cmd.Parameters.AddWithValue(DbParamMap[i], (string)data[i]);
                        break;
                }
            }
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while inserting values.");
            }
            finally
            {
                Con.Close();
            }
        }

        public void AddPersonWithoutId(ref Person p) // add without identity_insert
        {
            int insertedId = -1;
            Cmd.CommandText = "insert into dbo.Person values (@FirstName, @LastName, @Age, @Gender) " +
                "SELECT CAST(SCOPE_IDENTITY() AS INT)";
            Cmd.Parameters.Clear();
            ArrayList data = new ArrayList
            {
                p.FirstName, p.LastName, p.Age, p.Gender.ToString()
            };
            for (int i = 1; i < 5; i++)
            {
                int j = i - 1;
                switch (i)
                {
                    case int n when n == 1 || n == 2 || n == 4:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (string)data[j]);
                        break;
                    case 3:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (int)data[j]);
                        break;
                }
            }
            try
            {
                Con.Open();
                insertedId = (int)Cmd.ExecuteScalar();
            }
            catch (Exception e)
            {
                Helper.PrintError(e);
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while inserting values.");
            }
            finally
            {
                Con.Close();
            }
            p.Pid = p.Address.Pid = p.Phone.Pid = p.Email.Pid = insertedId;
            Cmd.CommandText = "insert into dbo.Address values (@Pid, @HouseNum, @Street, @City, @State, @Country, @ZipCode) " +
                "insert into dbo.Phone values (@Pid, @CountryCode, @Number, @Ext) " +
                "insert into dbo.Email values (@Pid, @EmailAddress)";
            Cmd.Parameters.Clear();
            Cmd.Parameters.AddWithValue(DbParamMap[0], p.Pid);
            data = new ArrayList
            {
                p.Address.HouseNum, p.Address.Street, p.Address.City, p.Address.State, p.Address.Country, p.Address.ZipCode,
                p.Phone.CountryCode, p.Phone.Number, p.Phone.Ext, p.Email.EmailAddress
            };
            for (int i = 5; i < DbParamMap.Count; i++)
            {
                int j = i - 5;
                switch (i)
                {
                    case int n when n >= 5 && n <= 10:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (string)data[j]);
                        break;
                    case 11:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (int)data[j]);
                        break;
                    case 12:
                        Cmd.Parameters.AddWithValue(DbParamMap[i], (long)data[j]);
                        break;
                    case 13:
                        if ((int?)data[j] == null)
                            Cmd.Parameters.AddWithValue(DbParamMap[i], DBNull.Value);
                        else
                            Cmd.Parameters.AddWithValue(DbParamMap[i], (int)data[j]);
                        break;
                    case 14:
                        if ((string)data[j] == null)
                            Cmd.Parameters.AddWithValue(DbParamMap[i], DBNull.Value);
                        else
                            Cmd.Parameters.AddWithValue(DbParamMap[i], (string)data[j]);
                        break;
                }
            }
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Helper.PrintError(e);
                Console.WriteLine("Error occured while inserting values.");
            }
            finally
            {
                Con.Close();
            }
        }

        public Person ReadPerson(long Pid) // read
        {
            Cmd.CommandText = "select per.Pid, FirstName, LastName, Age, Gender, HouseNum, Street, City, State, Country, ZipCode, CountryCode, Number, Ext, EmailAddress " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid " +
                "inner join dbo.Phone as pho on per.Pid = pho.Pid inner join dbo.Email as email on per.Pid = email.Pid " +
                "where per.Pid = @Pid";
            Cmd.Parameters.Clear();
            Cmd.Parameters.AddWithValue(DbParamMap[0], Pid);
            Person p = null;
            try
            {
                Con.Open();
                Reader = Cmd.ExecuteReader();
                if (Reader.Read())
                {
                    p = ParseEntry();
                }
                else
                    Console.WriteLine($"Person with Pid {Pid} does not exist!");
                return p;
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while reading the entry.");
                return null;
            }
            finally
            {
                Reader.Close();
                Con.Close();
            }
        }

        public void UpdatePerson(long Pid, int property, string val) // update
        {
            switch (property)
            {
                case int n when n <= 0 || n > 14:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    break;
                case int n when n <= 4:
                    Cmd.CommandText = "update dbo.Person";
                    break;
                case int n when n <= 10:
                    Cmd.CommandText = "update dbo.Address";
                    break;
                case int n when n <= 13:
                    Cmd.CommandText = "update dbo.Phone";
                    break;
                case 14:
                    Cmd.CommandText = "update dbo.Email";
                    break;
            }
            Cmd.CommandText += $" set {Helper.PropertyMap[property]} = @val where Pid = @Pid";
            Cmd.Parameters.Clear();
            if ((property == 13 || property == 14) && string.IsNullOrWhiteSpace(val))
                Cmd.Parameters.AddWithValue("@val", DBNull.Value);
            else
                Cmd.Parameters.AddWithValue("@val", val);
            Cmd.Parameters.AddWithValue(DbParamMap[0], Pid);
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while updating the entry.");
            }
            finally
            {
                Con.Close();
            }
        }

        public void DeletePerson(long Pid) // delete
        {
            Cmd.CommandText = "delete from dbo.Person where Pid = @Pid";
            Cmd.Parameters.Clear();
            Cmd.Parameters.AddWithValue(DbParamMap[0], Pid);
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while deleting the entry.");
            }
            finally
            {
                Con.Close();
            }
        }

        public ICollection<Person> SearchPerson(int property, string val) // search
        {
            Cmd.CommandText = "select per.Pid, FirstName, LastName, Age, Gender, " +
                "HouseNum, Street, City, State, Country, ZipCode, " +
                "CountryCode, Number, Ext, " +
                "EmailAddress " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid " +
                "inner join dbo.Phone as pho on per.Pid = pho.Pid " +
                "inner join dbo.Email as email on per.Pid = email.Pid ";
            switch (property)
            {
                case 1: // first name
                case 2: // last name
                    Cmd.CommandText += $"where {Helper.PropertyMap[property]} like @val";
                    break;
                case 3: // zip code
                    property = 10;
                    Cmd.CommandText += $"where {Helper.PropertyMap[property]} like @val";
                    break;
                case 4: // city
                    property = 7;
                    Cmd.CommandText += $"where {Helper.PropertyMap[property]} like @val";
                    break;
                case 5: // full phone number
                    Cmd.CommandText += "where concat(CountryCode, Number) like @val";
                    break;
                default:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    return null;
            }
            Cmd.Parameters.Clear();
            Cmd.Parameters.AddWithValue("@val", $"%{val}%");
            ICollection<Person> l = new List<Person>();
            try
            {
                Con.Open();
                Reader = Cmd.ExecuteReader();
                while (Reader.Read())
                {
                    Person p = ParseEntry();
                    l.Add(p);
                }
                return l;
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error occured while searching for entries.");
                return null;
            }
            finally
            {
                Reader.Close();
                Con.Close();
            }
        }

        public ContactDirectory LoadEntries() // for loading existing entries in database
        {
            ContactDirectory cd = new ContactDirectory();
            Cmd.CommandText = "select per.Pid, FirstName, LastName, Age, Gender, HouseNum, Street, City, State, Country, ZipCode, CountryCode, Number, Ext, EmailAddress " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid " +
                "inner join dbo.Phone as pho on per.Pid = pho.Pid inner join dbo.Email as email on per.Pid = email.Pid " +
                "order by per.Pid";
            try
            {
                Con.Open();
                Reader = Cmd.ExecuteReader();
                while (Reader.Read())
                {
                    Person p = ParseEntry();
                    cd.AddPerson(p);
                }
                return cd;
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error loading database entries.");
                Helper.PrintError(e);
                return null;
            }
            finally
            {
                Reader.Close();
                Con.Close();
            }
        }

        public void ClearEntries() // for deleting every entry in database
        {
            Cmd.CommandText = "delete dbo.Person delete dbo.Address delete dbo.Phone delete dbo.Email";
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Error clearing database entries.");
            }
            finally
            {
                Con.Close();
            }
        }

        public void ShowAllPerson() // show all existing entries in database
        {
            ContactDirectory cd = LoadEntries();
            Helper.PrintLegend();
            foreach (Person p in cd.People)
            {
                Console.WriteLine(p.ToString());
            }
        }

        private Person ParseEntry() // parse an entry from database (Reader's position in the current result set)
        {
            Person p = new Person();
            p.Pid = p.Address.Pid = p.Phone.Pid = p.Email.Pid = (long)Reader[0];
            p.FirstName = (string)Reader[1];
            p.LastName = (string)Reader[2];
            p.Age = (int)Reader[3];
            p.Gender = (Gender)Enum.Parse(typeof(Gender), (string)Reader[4], true);
            p.Address.HouseNum = (string)Reader[5];
            p.Address.Street = (string)Reader[6];
            p.Address.City = (string)Reader[7];
            p.Address.State = (string)Reader[8];
            p.Address.Country = (string)Reader[9];
            p.Address.ZipCode = (string)Reader[10];
            p.Phone.CountryCode = (int)Reader[11];
            p.Phone.Number = (long)Reader[12];
            p.Phone.Ext = Reader[13] != DBNull.Value ? (int?)Reader[13] : null;
            p.Email.EmailAddress = Reader[14] != DBNull.Value ? (string)Reader[14] : null;
            return p;
        }

        public class ContactMeMessage
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Message { get; set; }

            public ContactMeMessage() { }
        }

        public void AddMessage(ContactMeMessage m)
        {
            Cmd.CommandText = "insert into dbo.ContactMe values (@Name, @Email, @Message)";
            Cmd.Parameters.Clear();
            Cmd.Parameters.AddWithValue("@Name", m.Name);
            Cmd.Parameters.AddWithValue("@Email", m.Email);
            Cmd.Parameters.AddWithValue("@Message", m.Message);
            try
            {
                Con.Open();
                Cmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                logger.Error(e.StackTrace);
                Console.WriteLine("Inserting into dbo.ContactMe failed!");
            }
            finally
            {
                Con.Close();
            }
        }
    }
}
