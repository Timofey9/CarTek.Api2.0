namespace CarTek.Api.Model.Dto
{
    public class ExternalTransporterModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public ICollection<DriverModel> Drivers { get; set; }
        public ICollection<CarModel> Cars { get; set; }
        //public ICollection<OrderModel> Orders{ get; set; }
    }
}
