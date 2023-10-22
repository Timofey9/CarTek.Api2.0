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

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

        public string Material { get; set; }
        public DateTime StartDate { get; set; }

        public int Volume { get; set; }

        public Unit Unit { get; set; }

        public long OrderId { get; set; }

        public string AdminComment { get; set; }

        public DateTime DateCreated { get; set; }

        public long DriverId { get; set; }

        public DriverModel Driver { get; set; }

        public CarModel Car { get; set; }

        public ICollection<DriverTaskNote> Notes { get; set; }

        public int SubTasksCount { get; set; }

        public TNModel TN { get; set; }
    }
}
