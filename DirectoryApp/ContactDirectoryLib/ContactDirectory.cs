using System;
using System.Collections.Generic;
using System.Linq;

namespace ContactDirectoryLib
{
    public class ContactDirectory
    {
        public ICollection<Person> People { get; set; } // can be initialized to any generic collection
        // other data members
        private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger(); // for logging exceptions

        public ContactDirectory()
        {
            People = new List<Person>();
        }

        public ContactDirectory(ContactDirectory cd)
        {
            People = new List<Person>();
            foreach (Person p in cd.People)
            {
                People.Add(p);
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

        public void AddPerson(Person p)
        {
            People.Add(p);
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

        public void DeletePerson(long Pid)
        {
            Person p = ReadPerson(Pid);
            if (p != null)
                People.Remove(p);
        }

        public void UpdatePerson(long Pid, int property, string val)
        {
            Person p = ReadPerson(Pid);
            if (p != null)
            {
                switch (property)
                {
                    case int n when n <= 2:
                        p.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p, val);
                        break;
                    case 3:
                        p.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p, Int32.Parse(val));
                        break;
                    case 4:
                        p.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p, (Gender)Enum.Parse(typeof(Gender), val, true));
                        break;
                    case int n when n <= 10:
                        p.Address.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p.Address, val);
                        break;
                    case int n when n == 11 || n == 13:
                        p.Phone.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p.Phone, Int32.Parse(val));
                        break;
                    case 12:
                        p.Phone.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p.Phone, Int64.Parse(val));
                        break;
                    case 14:
                        p.Email.GetType().GetProperty(Helper.PropertyMap[property]).SetValue(p.Email, val);
                        break;
                    case int n when n <= 0 || n > 14:
                        Console.WriteLine("Invalid choice selected. No such property.");
                        break;
                }
            }
        }

        public ICollection<Person> SearchPerson(int property, string val)
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
                        where person.Address.ZipCode.ToString().Contains(val)
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
                        let pnum = person.Phone.CountryCode.ToString() +
                            person.Phone.Number.ToString()
                        where pnum.Contains(val)
                        select person;
                    break;
                default:
                    Console.WriteLine("Invalid choice selected. No such property.");
                    return null;
            }
            return query.ToList();
        }

        public void ShowAllPerson()
        {
            Helper.PrintLegend();
            foreach (Person p in People)
            {
                Console.WriteLine(p.ToString());
            }
        }
    }
}
