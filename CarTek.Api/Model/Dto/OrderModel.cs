using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model.Dto
{
    public class OrderModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public long GpId { get; set; }

        public ClientModel Client { get; set; }

        public ShiftType Shift { get; set; }
         
        public ClientModel Gp { get; set; }

        public string? ClientName { get; set; }

        public string? ClientInn { get; set; }

        public double Volume { get; set; }

        public Unit LoadUnit { get; set; }

        public Unit UnloadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime DueDate { get; set; }

        public string? LocationA { get; set; }
        public long? LocationAId { get; set; }

        public string? LocationB { get; set; }
        public long? LocationBId { get; set; }

        public double? Price { get; set; } // себестоимость перевозки

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
        public ExternalTransporterModel? ExternalTransporter { get; set; }

        public double? MaterialPrice { get; set; }  // себестоимость материала

        public string? Note { get; set; }

        public int CarCount { get; set; }

        public double? Density { get; set; }

        public string Mileage { get; set; }

        public long? MaterialId { get; set; }

        public ServiceType Service { get; set; }

        //Брать объем с выгрузки или с погрузки
        public ReportLoadType ReportLoadType { get; set; }

        //Услуга
        public MaterialModel Material { get; set; }

        public ICollection<DriverTaskOrderModel> DriverTasks { get; set; }
    }
}
