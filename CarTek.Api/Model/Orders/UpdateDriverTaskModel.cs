namespace CarTek.Api.Model.Orders
{
    public class UpdateDriverTaskModel
    {
        public long DriverTaskId { get; set; }

        public int UpdatedStatus { get; set; }

        public ICollection<IFormFile>? Files { get; set; }

        public string? Note { get; set; } 
    }
}
