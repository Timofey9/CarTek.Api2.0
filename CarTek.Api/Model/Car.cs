using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public enum TransportState
    {
        Base,   
        Line,
        Service
    }

    public enum TransportCondition
    {
        Damage,
        NoDamage
    }

    public class Car
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        //Количество осей
        public int AxelsCount { get; set; }

        public long CurrentOrderId { get; set; }

        ///////////////

        //На линии, на базе, в ремонте
        public TransportState State { get; set; }

        public Trailer? Trailer { get; set; }

        ///////////////
        public long? ExternalTransporterId { get; set; }
        public ExternalTransporter? ExternalTransporter { get; set; }
        //наемный водитель
        public bool IsExternal { get; set; }
        ///////////////

        public ICollection<DriverTask> DriverTasks { get; set; }

        public ICollection<Driver> Drivers { get; set; }

        public ICollection<Questionary> Questionaries { get; set; }
    }
}
