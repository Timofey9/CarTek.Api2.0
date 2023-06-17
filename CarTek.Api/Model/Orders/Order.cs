using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public enum Unit
    {
        m3,
        t,
        quantity
    }


    public enum ServiceType
    {
        Transport,
        Supply
    }

    public class Order
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string? ClientName { get; set; }

        public string? ClientInn { get; set; }

        public double Volume { get; set; }
        
        public Unit LoadUnit { get; set; }

        public Unit UnloadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public string? LocationA { get; set; }   

        public string? LocationB { get; set; }   
        
        public double Price { get; set; }

        public string? Note { get; set; }

        public int CarCount { get; set; }

        public long MaterialId { get; set; }

        public ServiceType Service { get; set; }


        //Услуга
        public Material Material { get; set; }

        public ICollection<DriverTask> DriverTasks { get; set; }
    }
}
