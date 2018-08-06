//using Newtonsoft.Json;

namespace ContactDirectoryLib
{
    public class Address
    {
        //[JsonIgnore]
        public long Pid { get; set; }
        public string HouseNum { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string ZipCode { get; set; }
    }
}
