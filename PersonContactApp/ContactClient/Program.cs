using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using ContactLibrary;

namespace ContactClient
{

    class Program
    {
        static string FileName = "";
        static FileStream fs = null;
        static ContactDirectory cd = null;

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome, what would you like to do?");
            PrintIntroPrompt();
            Console.WriteLine();
            while (true)
            {
                PrintPrompt();
                Console.WriteLine();
            }
        }

        static void PrintIntroPrompt()
        {
            Console.WriteLine("1. Create a new contact directory");
            Console.WriteLine("2. Load from existing contact directory");
            Console.Write("Enter your choice: ");
            switch (Console.ReadLine())
            {
                case "1": // new file
                    Console.Write("Specify file name: ");
                    FileName = Console.ReadLine();
                    fs = File.Create(FileName);
                    cd = new ContactDirectory();
                    break;
                case "2": // existing file
                    Console.Write("Specify file name: ");
                    FileName = Console.ReadLine();
                    try
                    {
                        fs = File.OpenRead(FileName);
                        if (new FileInfo(FileName).Length != 0)
                            cd = DeserializeFromFile(fs);
                        foreach (Person p in cd.People)
                        {
                            cd.AddPersonToDatabase(p);
                        }
                    }
                    catch (FileNotFoundException e)
                    {
                        Console.WriteLine($"File with {FileName} not found!\n");
                        PrintIntroPrompt();
                    }
                    break;
                default: // invalid arg
                    Console.WriteLine("Invalid choice, please try again.\n");
                    PrintIntroPrompt();
                    break;
            }
        }

        static void PrintPrompt()
        {
            int i, j;
            Console.WriteLine("1. Add person");
            Console.WriteLine("2. Read person");
            Console.WriteLine("3. Delete person");
            Console.WriteLine("4. Update person");
            Console.WriteLine("5. Search person");
            Console.WriteLine("6. Exit the app");
            Console.Write("Enter your choice: ");
            switch (Console.ReadLine())
            {
                case "1": // add
                    Person p = new Person();
                    askforid: Console.Write("Id: ");
                    p.Pid = Int64.Parse(Console.ReadLine());
                    foreach (Person per in cd.People)
                    {
                        if (per.Pid == p.Pid)
                        {
                            Console.WriteLine("A person with that Id already exists. Please try again.");
                            goto askforid;
                        }
                    }
                    p.Address.Pid = p.Pid;
                    p.Phone.Pid = p.Pid;
                    Console.Write("First Name: ");
                    p.FirstName = Console.ReadLine();
                    Console.Write("Last Name: ");
                    p.LastName = Console.ReadLine();
                    Console.Write("House Number: ");
                    p.Address.HouseNum = Console.ReadLine();
                    Console.Write("Street: ");
                    p.Address.Street = Console.ReadLine();
                    Console.Write("City: ");
                    p.Address.City = Console.ReadLine();
                    Console.Write("State: ");
                    p.Address.State = Console.ReadLine();
                    Console.Write("Country: ");
                    p.Address.Country = Console.ReadLine();
                    Console.Write("Zip Code: ");
                    p.Address.ZipCode = Console.ReadLine();
                    Console.Write("Country Code: ");
                    p.Phone.CountryCode = Console.ReadLine();
                    Console.Write("Area Code: ");
                    p.Phone.AreaCode = Console.ReadLine();
                    Console.Write("Phone Number: ");
                    p.Phone.Number = Console.ReadLine();
                    Console.Write("Extension: ");
                    p.Phone.Ext = Console.ReadLine();
                    cd.AddPerson(p);
                    SerializeToFile(cd, FileName);
                    cd.AddPersonToDatabase(p);
                    Console.WriteLine("Successfully added.");
                    break;
                case "2": // read
                    Console.Write("Specify the Pid: ");
                    try
                    {
                        //Console.WriteLine(cd.ReadPerson(Int32.Parse(Console.ReadLine())).ToString());
                        Console.WriteLine(cd.ReadPersonFromDatabase(Int32.Parse(Console.ReadLine())));
                    }
                    catch (NullReferenceException e)
                    {
                        Console.WriteLine("Person with this Id does not exist.");
                    }
                    break;
                case "3": // delete
                    Console.Write("Specify the Pid: ");
                    cd.DeletePerson(Int32.Parse(Console.ReadLine()));
                    SerializeToFile(cd, FileName);
                    cd.DeletePersonFromDatabase(Int32.Parse(Console.ReadLine()));
                    Console.WriteLine("Successfully deleted.");
                    break;
                case "4": // update
                    Console.Write("Specify the Pid: ");
                    i = Int32.Parse(Console.ReadLine());
                    Console.WriteLine("What do you want to update?");
                    Console.WriteLine("1. First Name");
                    Console.WriteLine("2. Last Name");
                    Console.WriteLine("3. House Number");
                    Console.WriteLine("4. Street");
                    Console.WriteLine("5. City");
                    Console.WriteLine("6. State");
                    Console.WriteLine("7. Country");
                    Console.WriteLine("8. Zip Code");
                    Console.WriteLine("9. Country Code");
                    Console.WriteLine("10. Area Code");
                    Console.WriteLine("11. Phone Number");
                    Console.WriteLine("12. Extension");
                    Console.Write("Enter your choice: ");
                    j = Int32.Parse(Console.ReadLine());
                    Console.Write("Enter the new value: ");
                    cd.UpdatePerson(i, j, Console.ReadLine());
                    SerializeToFile(cd, FileName);
                    cd.UpdatePersonInDatabase(i, j, Console.ReadLine());
                    Console.WriteLine("Successfully updated.");
                    break;
                case "5": // search
                    Console.WriteLine("What do you want to search by?");
                    Console.WriteLine("1. First Name");
                    Console.WriteLine("2. Last Name");
                    Console.WriteLine("3. Zip Code");
                    Console.WriteLine("4. City");
                    Console.WriteLine("5. Phone number");
                    Console.Write("Enter your choice: ");
                    i = Int32.Parse(Console.ReadLine());
                    Console.Write("Enter the value: ");
                    //foreach (Person person in cd.SearchPerson(i, Console.ReadLine()))
                    //{
                    //    Console.WriteLine(person.ToString());
                    //}
                    Console.WriteLine(cd.SearchPersonInDatabase(i, Console.ReadLine()));
                    break;
                case "6": // exit app
                    fs.Close();
                    Environment.Exit(0);
                    break;
                default: // invalid arg
                    Console.WriteLine("Invalid choice, please try again.\n");
                    PrintPrompt();
                    break;
            }
        }

        private static void SerializeToFile(ContactDirectory cd, string filename)
        {
            FileStream fs = File.Create(filename);
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(ContactDirectory));
            s.WriteObject(fs, cd);
        }

        private static ContactDirectory DeserializeFromFile(FileStream fs)
        {
            DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(ContactDirectory));
            return (ContactDirectory)s.ReadObject(fs);
        }

        //static void TestSerialize<T>(T t)
        //{
        //    MemoryStream ms = new MemoryStream();
        //    DataContractJsonSerializer dcjs = new DataContractJsonSerializer(typeof(T));
        //    dcjs.WriteObject(ms, t);
        //    ms.Position = 0;
        //    StreamReader sr = new StreamReader(ms);
        //    Console.WriteLine(sr.ReadToEnd());
        //}
    }
}
