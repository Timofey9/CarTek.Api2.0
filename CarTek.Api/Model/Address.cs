using System.ComponentModel.DataAnnotations.Schema;

namespace CarTek.Api.Model
{
    public class Address
    {
        public long Id { get; set; }    
        //public string Name { get; set; }
        public string Coordinates { get; set; }
        public string TextAddress { get; set; }

        [InverseProperty("LocationA")]
        public ICollection<TN> TNLocationA{ get; set; }
        [InverseProperty("LocationB")]
        public ICollection<TN> TNLocationB { get; set; }

    }

    public class CrateAddressModel
    {
        public long? Id { get; set; }

        //public string Name { get; set; }
        public string Coordinates { get; set; }
        public string TextAddress { get; set; }
    }
}
