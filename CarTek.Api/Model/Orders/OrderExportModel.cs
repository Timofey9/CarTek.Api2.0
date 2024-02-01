using CarTek.Api.Model.Dto;

namespace CarTek.Api.Model.Orders
{
    public class OrderExportModel
    {
        public long Id { get; set; }

        public string Name { get; set; }

        //Грузоотправитель
        public ClientModel Client { get; set; }

        public ShiftType Shift { get; set; }

        //Грузополучатель
        public ClientModel Gp { get; set; }

        public double? Volume { get; set; }

        public Unit LoadUnit { get; set; }

        public Unit UnloadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public Address LocationA { get; set; }

        public Address LocationB { get; set; }

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
        public ExternalTransporterModel? ExternalTransporter { get; set; }

        public double? MaterialPrice { get; set; }

        public string? Note { get; set; }

        public double? Density { get; set; }

        public int CarCount { get; set; }

        public int? Mileage { get; set; }

        public ServiceType Service { get; set; }

        //Услуга
        public MaterialModel Material { get; set; }

        public ICollection<DriverTaskOrderModel> DriverTasks { get; set; }
    }
}
