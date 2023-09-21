using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public class AdminUpdateTaskModel
    {
        public long TaskId { get; set; }

        public long? DriverId { get; set; }

        public long? CarId { get; set; }

        public string? AdminComment { get; set; }

        public DateTime? StartDate { get; set; }

        public ShiftType? Shift { get; set; }
    }
}