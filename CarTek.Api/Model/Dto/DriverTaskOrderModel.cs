using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class DriverTaskOrderModel
    {
        public long Id { get; set; }

        public Guid UniqueId { get; set; }

        // night/day
        public ShiftType Shift { get; set; }

        public DriverTaskStatus Status { get; set; }

        public DateTime StartDate { get; set; }

        public int Volume { get; set; }

        public Unit Unit { get; set; }

        public long OrderId { get; set; }

        public long DriverId { get; set; }

        public long CarId { get; set; }

        public DriverModel Driver { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }
    }
}
