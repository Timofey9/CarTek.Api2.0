namespace CarTek.Api.Model
{
    public class AdminUpdateTaskModel
    {
        public long TaskId { get; set; }

        public long? DriverId { get; set; }

        public long? CarId { get; set; }
    }
}