using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class CreateDriverTaskModel
    {
        public long OrderId { get; set; }

        public ShiftType Shift { get; set; }
        
        public long DriverId { get; set; }

        public long CarId { get; set; }
    }
}
