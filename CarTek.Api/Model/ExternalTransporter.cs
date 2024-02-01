namespace CarTek.Api.Model
{
    public class ExternalTransporter
    {
        public long Id { get; set; }

        public string Name { get; set; }


        public ICollection<Driver> Drivers { get; set; }
        public ICollection<Car> Cars { get; set; }
        public ICollection<Order> Orders { get; set; }
    }
}
