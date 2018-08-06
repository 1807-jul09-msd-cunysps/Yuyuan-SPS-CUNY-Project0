//using Newtonsoft.Json;

namespace ContactDirectoryLib
{
    public class Phone
    {
        //[JsonIgnore]
        public long Pid { get; set; }
        public int CountryCode { get; set; }
        //public string AreaCode { get; set; }
        public long Number { get; set; }
        public int? Ext { get; set; }
    }
}
