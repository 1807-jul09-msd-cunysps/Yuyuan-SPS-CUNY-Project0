using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ContactDirectoryLib
{
    public static class Helper
    {
        public static readonly Dictionary<int, string> PropertyMap = new Dictionary<int, string>()
        {
            { 0, "Id" }, { 1, "FirstName" }, { 2, "LastName" }, { 3, "Age" }, { 4,"Gender" }, { 5, "HouseNum" },
            { 6, "Street" }, { 7, "City" }, { 8, "State" }, { 9, "Country" }, { 10, "ZipCode" },
            { 11, "CountryCode" }, { 12, "Number" }, { 13, "Ext" }, { 14, "EmailAddress" }
        };

        public static readonly string PrettyFormat = "{0,4} | {1,14} | {2,14} | {3,4} | {4,6} | {5,7} | {6,18} | {7,20} | {8,20} | {9,10} | {10,7} | {11,10} | {12,13} | {13,4} | {14,18}";

        public static void PrintLegend()
        {
            Console.WriteLine(PrettyFormat,
                            "Pid", "First Name", "Last Name", "Age", "Gender", "House #", "Street", "City", "State", "Country", "Zip", "Country #", "Phone #", "Ext", "Email");
        }

        public static void PrintError(Exception e)
        {
            Console.WriteLine(e.Message + Environment.NewLine + e.StackTrace);
        }
    }

    public static class JsonHelper
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
}
