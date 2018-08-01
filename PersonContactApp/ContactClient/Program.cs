using System;
using System.Collections.Generic;
using System.IO;
using ContactLibrary;

namespace ContactClient
{
    class Program
    {
        private static string fileName = "example.txt"; // default file name for option 2
        private static ContactDirectory cd = null;

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
                        cd.ClearDatabaseEntries();
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
                            cd.ClearDatabaseEntries();
                            foreach (Person p in cd.People)
                            {
                                cd.AddPersonToDatabase(p);
                            }
                        }
                        else
                        {
                            cd = new ContactDirectory();
                            cd.LoadDatabaseEntries();
                        }
                    }
                    catch (Exception e)
                    {
                        cd.logger.Error(e.StackTrace);
                        if (option == 1)
                            Console.WriteLine("Error occured while attempting to deserialize the existing file." + Environment.NewLine);
                        else
                            Console.WriteLine("Error occured while executing SQL commands." + Environment.NewLine);
                        PrintIntroPrompt(option);
                    }
                    break;
                case "3": // exit app
                    if (option == 2 && cd.db.con != null)
                        cd.db.con.Close();
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
        }

        static void ShowSearchPrompt()
        {
            Console.WriteLine("What do you want to search by?");
            Console.WriteLine("1. First Name");
            Console.WriteLine("2. Last Name");
            Console.WriteLine("3. Zip Code");
            Console.WriteLine("4. City");
            Console.WriteLine("5. Full Phone Number");
            Console.Write("Enter your choice: ");
        }

        static void PrintPrompt(int option)
        {
            long i = 0, j = 0;
            int prop = 0;
            string value = "";
            bool b = true;
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
                        do
                        {
                            Console.Write("Id: ");
                            b = Int64.TryParse(Console.ReadLine(), out i);
                        } while (!b || !cd.ValidateID(i));
                        Person p = new Person();
                        p.Pid = p.Address.Pid = p.Phone.Pid = i;
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
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        if (option == 2)
                            cd.AddPersonToDatabase(p);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Add operation failed!");
                        cd.logger.Error(e.StackTrace);
                    }
                    break;
                case "2": // read
                    try
                    {
                        do
                        {
                            Console.Write("Specify the Pid: ");
                            b = Int64.TryParse(Console.ReadLine(), out i);
                        } while (!b);
                        cd.PrintLegend();
                        if (option == 1)
                            Console.WriteLine(cd.ReadPerson(i).ToString());
                        else
                            Console.WriteLine(cd.ReadPersonFromDatabase(i));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Read operation failed!");
                        cd.logger.Error(e.StackTrace);
                    }
                    break;
                case "3": // delete
                    try
                    {
                        do
                        {
                            Console.Write("Specify the Pid: ");
                            b = Int64.TryParse(Console.ReadLine(), out i);
                        } while (!b || cd.ReadPerson(i) == null);
                        cd.DeletePerson(i);
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        if (option == 2)
                            cd.DeletePersonFromDatabase(i);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Delete operation failed!");
                        cd.logger.Error(e.StackTrace);
                    }
                    break;
                case "4": // update
                    try
                    {
                        do
                        {
                            Console.Write("Specify the Pid: ");
                            b = Int64.TryParse(Console.ReadLine(), out i);
                        } while (!b || cd.ReadPerson(i) == null);
                        ShowUpdatePrompt();
                        do
                        {
                            b = Int32.TryParse(Console.ReadLine(), out prop);
                        } while (!b);
                        Console.Write("Enter the new value: ");
                        value = Console.ReadLine();
                        cd.UpdatePerson(i, prop, value);
                        File.WriteAllText(fileName, JsonHelper.JsonSerializer(cd));
                        if (option == 2)
                            cd.UpdatePersonInDatabase(i, prop, value);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Update operation failed!");
                        cd.logger.Error(e.StackTrace);
                    }
                    break;
                case "5": // search
                    try
                    {
                        ShowSearchPrompt();
                        do
                        {
                            b = Int32.TryParse(Console.ReadLine(), out prop);
                        } while (!b);
                        Console.Write("Enter the value: ");
                        value = Console.ReadLine();
                        cd.PrintLegend();
                        if (option == 1)
                        {
                            List<Person> l = cd.SearchPerson(prop, value);
                            if (l != null)
                            {
                                foreach (Person person in l)
                                {
                                    Console.WriteLine(person.ToString());
                                }
                            }
                        }
                        else
                            Console.WriteLine(cd.SearchPersonInDatabase(prop, value));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Search operation failed!");
                        cd.logger.Error(e.StackTrace);
                    }
                    break;
                case "6": // show all contacts
                    cd.ShowAllPerson();
                    break;
                case "7": // exit app
                    if (option == 2)
                        cd.db.con.Close();
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
