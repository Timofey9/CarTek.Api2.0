using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public enum Unit
    {
        m3,
        t,
        none
    }

    public enum ServiceType
    {
        Transport,
        Supply
    }

    public class Order
    {
        public long Id { get; set; }

        public string? Name { get; set; }

        public string? ClientName { get; set; }

        public double? Volume { get; set; }
        
        public ShiftType Shift { get; set; }

        public Unit? LoadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? Mileage { get; set; }

        public string? LocationA { get; set; }   

        public long? LocationAId { get; set; }   

        public string? LocationB { get; set; }

        public long? LocationBId { get; set; }

        /// <summary>
        /// себестоимость перевозки (КарТэк)
        /// </summary>
        public double? Price { get; set; } 

        /// <summary>
        /// себестоимость перевозки (наемник)
        /// </summary>
        public double? ExternalPrice { get; set; } 

        /// <summary>
        /// Cебестоимость перевозки (водитель)
        /// </summary>
        public double? DriverPrice { get; set; } 

        /// <summary>
        /// В зависимости от типа заявки: Price - Driver/ExternalPrice
        /// </summary>
        public double? Discount { get; set; }

        /// <summary>
        /// Для наемного транспорта/водителя
        /// </summary>
        public bool IsExternal { get; set; }
        public long? ExternalTransporterId { get; set; }
        public ExternalTransporter? ExternalTransporter { get; set; }

        /// <summary>
        /// себестоимость материала
        /// </summary>
        public double? MaterialPrice { get; set; } 

        public string? Note { get; set; }

        public int CarCount { get; set; }

        public long? MaterialId { get; set; }

        public long? ClientId { get; set; }

        public long GpId { get; set; }

        public double? Density { get; set; }

        public ServiceType Service { get; set; }

        public Material Material { get; set; }

        public Client Client { get; set; }

        public ICollection<DriverTask> DriverTasks { get; set; }
        public ICollection<SubTask> SubTasks { get; set; }
    }
}
