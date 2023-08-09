namespace CarTek.Api.Model
{
    public class Address
    {
        public long Id { get; set; }    
        public string Name { get; set; }
        public string Coordinates { get; set; }
        public string TextAddress { get; set; }
    }

    public class CrateAddressModel
    {
        public string Name { get; set; }
        public string Coordinates { get; set; }
        public string TextAddress { get; set; }
    }
}
