namespace CarTek.Api.Model.Orders
{
    public class UpdateDriverTaskModel
    {
        public long DriverTaskId { get; set; }

        public int UpdatedStatus { get; set; }

        public IFormFile File { get; set; }

        public string Note { get; set; } 
    }
}
