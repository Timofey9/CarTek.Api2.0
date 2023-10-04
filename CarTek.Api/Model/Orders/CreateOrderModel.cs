namespace CarTek.Api.Model.Orders
{
    public class CreateOrderModel
    {
        public string Name { get; set; }

        public string ClientName { get; set; }
 
        public long MaterialId { get; set; }

        public double? Volume { get; set; }

        public ShiftType Shift { get; set; }

        public Unit LoadUnit { get; set; }

        public bool IsComplete { get; set; }

        public long AddressAId { get; set; }
        
        public long AddressBId { get; set; }
        
        //GO
        public long ClientId { get; set; }

        public long GpId { get; set; }

        public DateTime? DueDate { get; set; }
        
        public DateTime StartDate { get; set; }       
        
        public string? Mileage { get; set; }

        public double? Price { get; set; }
      
        public string? Note { get; set; }

        public int CarCount { get; set; }

        //Услуга
        public ServiceType Service { get; set; }
    }
}
