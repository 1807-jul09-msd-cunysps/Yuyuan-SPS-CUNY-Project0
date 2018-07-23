using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Serialization;

namespace ContactLibrary
{
    [DataContract] // want to serialize the directory class
    public class ContactDirectory
    {
        [DataMember] // the only data member that needs to be serialize
        public List<Person> People { get; set; }

        private SqlConnection con = null;
        private SqlCommand cmd = null;
        private SqlDataReader read = null;
        private NLog.Logger logger = null;

        public ContactDirectory()
        {
            People = new List<Person>();
            con = new SqlConnection("Data Source = chancunysps.database.windows.net; Initial Catalog = TestDb; Persist Security Info = True; User ID = cyy5113; Password = Password12345");
            con.Open();
            cmd = new SqlCommand("", con);
            logger = NLog.LogManager.GetCurrentClassLogger();
        }

        private readonly Dictionary<int, string> Mapping = new Dictionary<int, string>()
        { { 1, "FirstName" }, { 2, "LastName" }, { 3, "HouseNum" }, { 4, "Street" }, { 5, "City" },
            { 6, "State" }, { 7, "Country" }, { 8, "ZipCode" }, { 9, "CountryCode" }, { 10, "AreaCode" },
            { 11, "Number" }, { 12, "Ext" }, { 0, "Id" } };

        public void AddPerson(Person p)
        {
            People.Add(p);
        }

        public void AddPersonToDatabase(Person p)
        {
            cmd.CommandText = $"insert into dbo.Person (Pid, FirstName, LastName) values ({p.Pid}, '{p.FirstName}', '{p.LastName}') " +
                $"insert into dbo.Address (Pid, HouseNum, Street, City, State, Country, ZipCode) values " +
                $"({p.Pid}, '{p.Address.HouseNum}', '{p.Address.Street}', '{p.Address.City}', '{p.Address.State}', '{p.Address.Country}', '{p.Address.ZipCode}') " +
                $"insert into dbo.Phone (Pid, CountryCode, AreaCode, PhoneNumber, Extension) values " +
                $"({p.Pid}, '{p.Phone.CountryCode}', '{p.Phone.AreaCode}', '{p.Phone.Number}', '{p.Phone.Ext}')";
            cmd.ExecuteNonQuery();
        }

        public Person ReadPerson(int Pid)
        {
            IEnumerable<Person> query =
                from person in People
                where person.Pid.Equals(Pid)
                select person;
            if (!query.Any())
            {
                Console.WriteLine($"Person with Pid {Pid} does not exist.");
                return null;
            }
            return query.Single();
        }

        public string ReadPersonFromDatabase(int Pid)
        {
            cmd.CommandText = $"select per.Pid, FirstName, LastName, HouseNum, Street, City, State, Country, ZipCode, CountryCode, AreaCode, PhoneNumber, Extension " +
                $"from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid inner join dbo.Phone as pho on per.Pid = pho.Pid " +
                $"where per.Pid = {Pid}";
            read = cmd.ExecuteReader();
            read.Read();
            string result = "";
            for (int i = 0; i < read.FieldCount; ++i)
            {
                result += read[i] + "\t";
            }
            read.Close();
            return result;
        }

        public void DeletePerson(int Pid)
        {
            IEnumerable<Person> query =
                from person in People
                where person.Pid.Equals(Pid)
                select person;
            if (!query.Any())
            {
                Console.WriteLine($"Person with Pid {Pid} does not exist.");
                return;
            }
            People.Remove(query.Single());
        }

        public void DeletePersonFromDatabase(int Pid)
        {
            cmd.CommandText = $"delete from dbo.Person where Pid = {Pid}";
            cmd.ExecuteNonQuery();
        }

        public void UpdatePerson(int Pid, int attr, string val)
        {
            IEnumerable<Person> query =
                from person in People
                where person.Pid.Equals(Pid)
                select person;
            if (!query.Any())
            {
                Console.WriteLine($"Person with Pid {Pid} does not exist.");
                return;
            }
            Person p = query.Single();
            switch (attr)
            {
                case int n when n <= 2:
                    p.GetType().GetProperty(Mapping[attr]).SetValue(p, val);
                    break;
                case int n when n <= 8:
                    p.Address.GetType().GetProperty(Mapping[attr]).SetValue(p.Address, val);
                    break;
                case int n when n <= 12:
                    p.Phone.GetType().GetProperty(Mapping[attr]).SetValue(p.Phone, val);
                    break;
                case int n when n < 0 || n > 12:
                    Console.WriteLine("No such attribute.");
                    break;
            }
        }

        public void UpdatePersonInDatabase(int Pid, int attr, string val)
        {
            switch (attr)
            {
                case int n when n <= 2:
                    cmd.CommandText = $"update dbo.Person {Mapping[attr]} = {val} where Pid = {Pid}";
                    break;
                case int n when n <= 8:
                    cmd.CommandText = $"update dbo.Address {Mapping[attr]} = {val} where Pid = {Pid}";
                    break;
                case int n when n <= 12:
                    cmd.CommandText = $"update dbo.Phone {Mapping[attr]} = {val} where Pid = {Pid}";
                    break;
                case int n when n < 0 || n > 12:
                    Console.WriteLine("No such attribute.");
                    break;
            }
            cmd.ExecuteNonQuery();
        }

        public List<Person> SearchPerson(int attr, string val)
        {
            // firstname, lastname, zipcode, city, phone number
            // specify the query as the string param
            // say want to search for people with lastname "alex"
            // would call the function as follows
            // SearchPerson(2, "alex")
            IEnumerable<Person> query;
            switch (attr)
            {
                case 1:
                    query =
                        from person in People
                        where person.FirstName.Equals(val)
                        select person;
                    break;
                case 2:
                    query =
                        from person in People
                        where person.LastName.Equals(val)
                        select person;
                    break;
                case 3:
                    query =
                        from person in People
                        where person.Address.ZipCode.Equals(val)
                        select person;
                    break;
                case 4:
                    query =
                        from person in People
                        where person.Address.City.Equals(val)
                        select person;
                    break;
                case 5:
                    query =
                        from person in People
                        let phoneNumber = person.Phone.CountryCode +
                            person.Phone.AreaCode +
                            person.Phone.Number
                        where phoneNumber.Equals(val)
                        select person;
                    break;
                default:
                    Console.WriteLine("Query error, attribute does not exist");
                    return null;
            }
            return query.ToList();
        }

        public string SearchPersonInDatabase(int attr, string val)
        {
            cmd.CommandText = "select per.Pid, FirstName, LastName, " +
                "HouseNum, Street, City, State, Country, ZipCode, " +
                "CountryCode, AreaCode, PhoneNumber, Extension " +
                "from dbo.Person as per inner join dbo.Address as addr on per.Pid = addr.Pid " +
                "inner join dbo.Phone as pho on per.Pid = pho.Pid ";
            switch (attr)
            {
                case 1:
                case 2:
                    cmd.CommandText += $"where {Mapping[attr]} = '{val}'";
                    break;
                case 3:
                    attr = 8;
                    cmd.CommandText += $"where {Mapping[attr]} = '{val}'";
                    break;
                case 4:
                    attr = 5;
                    cmd.CommandText += $"where {Mapping[attr]} = '{val}'";
                    break;
                case 5:
                    cmd.CommandText += $"where concat(CountryCode, AreaCode, PhoneNumber) = '{val}'";
                    break;
                default:
                    Console.WriteLine("Query error, attribute does not exist");
                    return null;
            }
            read = cmd.ExecuteReader();
            string result = "";
            while (read.Read())
            {
                for (int i = 0; i < read.FieldCount; ++i)
                {
                    result += read[i] + "\t";
                }
                result += "\n";
            }
            read.Close();
            return result.TrimEnd('\t', '\n');
        }
    }

    public class Person
    {
        public Person()
        {
            /// Initialise the dependant objects
            Address = new Address();
            Phone = new Phone();
        }
        public long Pid { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Address Address { get; set; }
        public Phone Phone { get; set; }

        public override string ToString()
        {
            return Pid + " " + FirstName + " " + LastName + " " +
                Address.ToString() + " " + Phone.ToString();
        }
    }

    public class Address
    {
        public long Pid { get; set; }
        public string HouseNum { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }

        public override string ToString()
        {
            return HouseNum + " " + Street + ", " + City +
                ", " + ZipCode + " " + State + ", " + Country;
        }
    }

    public class Phone
    {
        public long Pid { get; set; }
        public string CountryCode { get; set; }
        public string AreaCode { get; set; }
        public string Number { get; set; }
        public string Ext { get; set; }

        public override string ToString()
        {
            return CountryCode + "-" + AreaCode + "-" + Number;
        }
    }

    //public enum State // only applies for United States, other countries won't be able to utilize it
    //{
    //    AL, AK, AZ, AR, CA, CO, CT, DE, FL, GA, HI, ID, IL, IN, IA, KS, KY, LA, ME, MD, MA, MI, MN, MS, MO, MT, NE, NV, NH, NJ, NM, NY, NC, ND, OH,
    //    OK, OR, PA, RI, SC, SD, TN, TX, UT, VT, VA, WA, WV, WI, WY, AS, DC, FM, GU, MH, MP, PW, PR, VI
    //}

    //public enum Country // should have unique values, some countries have the same country code, this causes bugs like UnitedStates = PuertoRico
    //{
    //    Afghanistan = 93, Albania = 355, Algeria = 213, AmericanSamoa = 1, Andorra = 376, Angola = 244, Anguilla = 1, Antarctica = 672,
    //    AntiguaandBarbuda = 1, Argentina = 54, Armenia = 374, Aruba = 297, Australia = 61, Austria = 43, Azerbaijan = 994, Bahamas = 1,
    //    Bahrain = 973, Bangladesh = 880, Barbados = 1, Belarus = 375, Belgium = 32, Belize = 501, Benin = 229, Bermuda = 1,
    //    Bhutan = 975, Bolivia = 591, BosniaandHerzegovina = 387, Botswana = 267, Brazil = 55, BritishIndianOceanTerritory = 246,
    //    BritishVirginIslands = 1, Brunei = 673, Bulgaria = 359, BurkinaFaso = 226, Burundi = 257, Cambodia = 855, Cameroon = 237, Canada = 1,
    //    CapeVerde = 238, CaymanIslands = 1, CentralAfricanRepublic = 236, Chad = 235, Chile = 56, China = 86, ChristmasIsland = 61,
    //    CocosIslands = 61, Colombia = 57, Comoros = 269, CookIslands = 682, CostaRica = 506, Croatia = 385, Cuba = 53, Curacao = 599, Cyprus = 357,
    //    CzechRepublic = 420, DemocraticRepublicoftheCongo = 243, Denmark = 45, Djibouti = 253, Dominica = 1, DominicanRepublic = 1, EastTimor = 670,
    //    Ecuador = 593, Egypt = 20, ElSalvador = 503, EquatorialGuinea = 240, Eritrea = 291, Estonia = 372, Ethiopia = 251, FalklandIslands = 500,
    //    FaroeIslands = 298, Fiji = 679, Finland = 358, France = 33, FrenchPolynesia = 689, Gabon = 241, Gambia = 220, Georgia = 995, Germany = 49,
    //    Ghana = 233, Gibraltar = 350, Greece = 30, Greenland = 299, Grenada = 1, Guam = 1, Guatemala = 502, Guernsey = 44, Guinea = 224,
    //    GuineaBissau = 245, Guyana = 592, Haiti = 509, Honduras = 504, HongKong = 852, Hungary = 36, Iceland = 354, India = 91, Indonesia = 62,
    //    Iran = 98, Iraq = 964, Ireland = 353, IsleofMan = 44, Israel = 972, Italy = 39, IvoryCoast = 225, Jamaica = 1, Japan = 81,
    //    Jersey = 44, Jordan = 962, Kazakhstan = 7, Kenya = 254, Kiribati = 686, Kosovo = 383, Kuwait = 965, Kyrgyzstan = 996, Laos = 856,
    //    Latvia = 371, Lebanon = 961, Lesotho = 266, Liberia = 231, Libya = 218, Liechtenstein = 423, Lithuania = 370, Luxembourg = 352, Macau = 853,
    //    Macedonia = 389, Madagascar = 261, Malawi = 265, Malaysia = 60, Maldives = 960, Mali = 223, Malta = 356, MarshallIslands = 692, Mauritania = 222,
    //    Mauritius = 230, Mayotte = 262, Mexico = 52, Micronesia = 691, Moldova = 373, Monaco = 377, Mongolia = 976, Montenegro = 382, Montserrat = 1,
    //    Morocco = 212, Mozambique = 258, Myanmar = 95, Namibia = 264, Nauru = 674, Nepal = 977, Netherlands = 31, NetherlandsAntilles = 599,
    //    NewCaledonia = 687, NewZealand = 64, Nicaragua = 505, Niger = 227, Nigeria = 234, Niue = 683, NorthKorea = 850, NorthernMarianaIslands = 1,
    //    Norway = 47, Oman = 968, Pakistan = 92, Palau = 680, Palestine = 970, Panama = 507, PapuaNewGuinea = 675, Paraguay = 595, Peru = 51,
    //    Philippines = 63, Pitcairn = 64, Poland = 48, Portugal = 351, PuertoRico = 1, Qatar = 974, RepublicoftheCongo = 242, Reunion = 262,
    //    Romania = 40, Russia = 7, Rwanda = 250, SaintBarthelemy = 590, SaintHelena = 290, SaintKittsandNevis = 1, SaintLucia = 1,
    //    SaintMartin = 590, SaintPierreandMiquelon = 508, SaintVincentandtheGrenadines = 1, Samoa = 685, SanMarino = 378, SaoTomeandPrincipe = 239,
    //    SaudiArabia = 966, Senegal = 221, Serbia = 381, Seychelles = 248, SierraLeone = 232, Singapore = 65, SintMaarten = 1, Slovakia = 421,
    //    Slovenia = 386, SolomonIslands = 677, Somalia = 252, SouthAfrica = 27, SouthKorea = 82, SouthSudan = 211, Spain = 34, SriLanka = 94, Sudan = 249,
    //    Suriname = 597, SvalbardandJanMayen = 47, Swaziland = 268, Sweden = 46, Switzerland = 41, Syria = 963, Taiwan = 886, Tajikistan = 992,
    //    Tanzania = 255, Thailand = 66, Togo = 228, Tokelau = 690, Tonga = 676, TrinidadandTobago = 1, Tunisia = 216, Turkey = 90, Turkmenistan = 993,
    //    TurksandCaicosIslands = 1, Tuvalu = 688, USVirginIslands = 1, Uganda = 256, Ukraine = 380, UnitedArabEmirates = 971, UnitedKingdom = 44,
    //    UnitedStates = 1, Uruguay = 598, Uzbekistan = 998, Vanuatu = 678, Vatican = 379, Venezuela = 58, Vietnam = 84, WallisandFutuna = 681,
    //    WesternSahara = 212, Yemen = 967, Zambia = 260, Zimbabwe = 263
    //}
}
