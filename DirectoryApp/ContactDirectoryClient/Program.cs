using System;
using System.Collections.Generic;
using System.IO;
using ContactDirectoryLib;
using DirectoryDAL;

namespace DirectoryClient
{
    class Program
    {
        private static string fileName = "example.txt"; // default file name for option 2
        private static ContactDirectory cd = null;
        private static DbHandler db = new DbHandler();
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome, what would you like to do?");
            // For option: 1 = serialization / deserialization only, 2 = option 1 + database
            int option = 2;
            PrintIntroPrompt(option);
            Console.WriteLine();
            while (true)
            {
                PrintPrompt(option);
                Console.WriteLine();
            }
        }

        static void PrintIntroPrompt(int option)
        {
            if (option == 1)
            {
                Console.WriteLine("1. Serialize to new file");
                Console.WriteLine("2. Deserialize from existing file");
                Console.WriteLine("3. Exit the application");
            }
            else
            {
                Console.WriteLine("1. Create a new contact directory");
                Console.WriteLine("2. Load from existing contact directory");
                Console.WriteLine("3. Exit the application");
            }
            Console.Write("Enter your choice: ");
            switch (Console.ReadLine())
            {
                case "1": // clear old entries, create new file
                    cd = new ContactDirectory();
                    if (option == 1)
                    {
                        Console.Write("Specify file name: ");
                        fileName = Console.ReadLine();
                    }
                    if (option == 2)
                        db.ClearEntries();
                    break;
                case "2": // load existing entries
                    if (option == 1)
                    {
                        Console.Write("Specify file name: ");
                        fileName = Console.ReadLine();
                    }
                    try
                    {
                        if (option == 1)
                        {
                            cd = new ContactDirectory(JsonHelper.JsonDeserializer<ContactDirectory>(File.ReadAllText(fileName)));
                            db.ClearEntries();
                            foreach (Person p in cd.People)
                            {
                                db.AddPerson(p);
                            }
                        }
                        else
                        {
                            cd = db.LoadEntries();
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error(e.StackTrace);
                        if (option == 1)
                            Console.WriteLine("Error occured while attempting to deserialize the existing file." + Environment.NewLine);
                        else
                            Console.WriteLine("Error occured while executing SQL commands." + Environment.NewLine);
                        PrintIntroPrompt(option);
                    }
                    break;
                case "3": // exit app
                    Environment.Exit(0);
                    break;
                default: // invalid arg
                    Console.WriteLine("Invalid choice, please try again." + Environment.NewLine);
                    PrintIntroPrompt(option);
                    break;
            }
        }

        static void ShowUpdatePrompt()
        {
            Console.WriteLine("What do you want to update?");
            Console.WriteLine("1. First Name");
            Console.WriteLine("2. Last Name");
            Console.WriteLine("3. Age");
            Console.WriteLine("4. Gender");
            Console.WriteLine("5. House Number");
            Console.WriteLine("6. Street");
            Console.WriteLine("7. City");
            Console.WriteLine("8. State");
            Console.WriteLine("9. Country");
            Console.WriteLine("10. Zip Code");
            Console.WriteLine("11. Country Code");
            Console.WriteLine("12. Phone Number");
            Console.WriteLine("13. Extension");
            Console.WriteLine("14. Email");
        }

        static void ShowSearchPrompt()
        {
            Console.WriteLine("What do you want to search by?");
            Console.WriteLine("1. First Name");
            Console.WriteLine("2. Last Name");
            Console.WriteLine("3. Zip Code");
            Console.WriteLine("4. City");
            Console.WriteLine("5. Full Phone Number");
        }

        static void PrintPrompt(int option)
        {
            long i = 0;
            int prop = 0, j = 0;
            string value = "";
            bool b = false;
            Console.WriteLine("1. Add person");
            Console.WriteLine("2. Read person");
            Console.WriteLine("3. Delete person");
            Console.WriteLine("4. Update person");
            Console.WriteLine("5. Search person");
            Console.WriteLine("6. Show every person");
            Console.WriteLine("7. Exit the application");
            Console.Write("Enter your choice: ");
            switch (Console.ReadLine())
            {
                case "1": // add
                    try
                    {
                        Console.WriteLine("Optional fields can be left blank.");
                        Person p = new Person();
                        //do
                        //{
                        //    b = false;
                        //    Console.Write("Id: ");
                        //    value = Console.ReadLine();
                        //    if (!String.IsNullOrWhiteSpace(value))
                        //        b = Int64.TryParse(value, out i);
                        //} while (!b || !cd.ValidateID(i));
                        //p.Pid = p.Address.Pid = p.Phone.Pid = i;
                        do
                        {
                            Console.Write("First Name: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.FirstName = value;
                        do
                        {
                            Console.Write("Last Name: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.LastName = value;
                        do
                        {
                            b = false;
                            Console.Write("Age: ");
                            value = Console.ReadLine();
                            if (!String.IsNullOrWhiteSpace(value))
                                b = Int32.TryParse(value, out j);
                        } while (!b);
                        p.Age = j;
                        do
                        {
                            b = false;
                            Console.Write("Gender (M, F, O): ");
                            switch (Console.ReadLine().ToUpper())
                            {
                                case "M":
                                    p.Gender = Gender.Male;
                                    b = true;
                                    break;
                                case "F":
                                    p.Gender = Gender.Female;
                                    b = true;
                                    break;
                                case "O":
                                    p.Gender = Gender.Other;
                                    b = true;
                                    break;
                                default:
                                    break;
                            }
                        } while (!b);
                        do
                        {
                            Console.Write("House Number: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.HouseNum = value;
                        do
                        {
                            Console.Write("Street: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.Street = value;
                        do
                        {
                            Console.Write("City: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.City = value;
                        do
                        {
                            Console.Write("State: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.State = value;
                        do
                        {
                            Console.Write("Country: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.Country = value;
                        do
                        {
                            Console.Write("Zip Code: ");
                            value = Console.ReadLine();
                        } while (String.IsNullOrWhiteSpace(value));
                        p.Address.ZipCode = value;
                        do
                        {
                            b = false;
                            Console.Write("Country Code: ");
                            value = Console.ReadLine();
                            if (!String.IsNullOrWhiteSpace(value))
                                b = Int32.TryParse(value, out j);
                        } while (!b);
                        p.Phone.CountryCode = j;
                        do
                        {
                            b = false;
                            Console.Write("Phone Number: ");
                            value = Console.ReadLine();
                            if (!String.IsNullOrWhiteSpace(value))
                                b = Int64.TryParse(value, out i);
                        } while (!b);
                        p.Phone.Number = i;
                        do
                        {
                            b = false;
                            Console.Write("Extension (optional): ");
                            value = Console.ReadLine();
                            if (String.IsNullOrWhiteSpace(value))
                            {
                                p.Phone.Ext = null;
                                break;
                            }
                            b = Int32.TryParse(value, out j);
                            if (!b)
                                continue;
                            p.Phone.Ext = j;
                            break;
                        } while (true);
                        Console.Write("Email (optional): ");
                        p.Email.EmailAddress = String.IsNullOrWhiteSpace(Console.ReadLine()) ? null : value;
                        if (option == 2)
                            db.AddPersonWithoutId(ref p);
                        cd.AddPerson(p);
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        Console.WriteLine("Successfully added.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message + e.StackTrace + "Add operation failed!");
                        logger.Error(e.StackTrace);
                    }
                    break;
                case "2": // read
                    try
                    {
                        Console.Write("Specify the Pid: ");
                        b = Int64.TryParse(Console.ReadLine(), out i);
                        if (!b || cd.ReadPerson(i) == null)
                            break;
                        Helper.PrintLegend();
                        if (option == 1)
                            Console.WriteLine(cd.ReadPerson(i).ToString());
                        else
                            Console.WriteLine(db.ReadPerson(i).ToString());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Read operation failed!");
                        logger.Error(e.StackTrace);
                    }
                    break;
                case "3": // delete
                    try
                    {
                        Console.Write("Specify the Pid: ");
                        b = Int64.TryParse(Console.ReadLine(), out i);
                        if (!b || cd.ReadPerson(i) == null)
                            break;
                        cd.DeletePerson(i);
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        if (option == 2)
                            db.DeletePerson(i);
                        Console.WriteLine("Successfully deleted.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Delete operation failed!");
                        logger.Error(e.StackTrace);
                    }
                    break;
                case "4": // update
                    try
                    {
                        Console.Write("Specify the Pid: ");
                        b = Int64.TryParse(Console.ReadLine(), out i);
                        if (!b || cd.ReadPerson(i) == null)
                            break;
                        ShowUpdatePrompt();
                        do
                        {
                            Console.Write("Enter your choice: ");
                            b = Int32.TryParse(Console.ReadLine(), out prop);
                        } while (!b);
                        switch (prop)
                        {
                            case int n when n == 3 || n == 10 || n == 11 || n == 13:
                                do
                                {
                                    b = false;
                                    Console.Write("Enter the new value: ");
                                    value = Console.ReadLine();
                                    if (!String.IsNullOrWhiteSpace(value))
                                        b = Int32.TryParse(value, out j);
                                } while (!b);
                                break;
                            case 4:
                                do
                                {
                                    Console.Write("Enter the new value (M, F, O): ");
                                    b = false;
                                    switch (Console.ReadLine().ToUpper())
                                    {
                                        case "M":
                                            value = Gender.Male.ToString();
                                            b = true;
                                            break;
                                        case "F":
                                            value = Gender.Female.ToString();
                                            b = true;
                                            break;
                                        case "O":
                                            value = Gender.Other.ToString();
                                            b = true;
                                            break;
                                        default:
                                            break;
                                    }
                                } while (!b);
                                break;
                            case 12:
                                do
                                {
                                    b = false;
                                    Console.Write("Enter the new value: ");
                                    value = Console.ReadLine();
                                    if (!String.IsNullOrWhiteSpace(value))
                                        b = Int64.TryParse(value, out long k);
                                } while (!b);
                                break;
                            default:
                                do
                                {
                                    Console.Write("Enter the new value: ");
                                    value = Console.ReadLine();
                                } while (String.IsNullOrWhiteSpace(value));
                                break;
                        }
                        cd.UpdatePerson(i, prop, value);
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        if (option == 2)
                            db.UpdatePerson(i, prop, value);
                        Console.WriteLine("Successfully updated.");
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Update operation failed!");
                        logger.Error(e.StackTrace);
                    }
                    break;
                case "5": // search
                    try
                    {
                        ShowSearchPrompt();
                        do
                        {
                            Console.Write("Enter your choice: ");
                            b = Int32.TryParse(Console.ReadLine(), out prop);
                        } while (!b);
                        Console.Write("Enter the value: ");
                        value = Console.ReadLine(); 
                        Helper.PrintLegend();
                        ICollection<Person> l = option == 1 ? cd.SearchPerson(prop, value) : db.SearchPerson(prop, value);
                        if (l != null)
                        {
                            foreach (Person person in l)
                            {
                                Console.WriteLine(person.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Search operation failed!");
                        logger.Error(e.StackTrace);
                    }
                    break;
                case "6": // show all contacts
                    if (option == 1)
                        cd.ShowAllPerson();
                    else
                        db.ShowAllPerson();
                    break;
                case "7": // exit app
                    Environment.Exit(0);
                    break;
                default: // invalid arg
                    Console.WriteLine("Invalid choice, please try again.\n");
                    PrintPrompt(option);
                    break;
            }
        }
    }
}
