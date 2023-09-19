using CarTek.Api.Model.Dto;
using CarTek.Api.Model.Orders;

namespace CarTek.Api.Model
{
    public enum Unit
    {
        m3,
        t,
        quantity,
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

        // public DateTime DateCreated { get; set; }

        //Грузополучатель - это компания, поэтому можно их брать из клиентов
        //Грузоотправитель - в случае Поставки - это картек, в случае перевозки -> Брать из клиентов, может быть null

        //Приемщик - просто строка (имя + номер телефона)

        public string? ClientName { get; set; }

        public double? Volume { get; set; }
        
        public Unit LoadUnit { get; set; }

        public bool IsComplete { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? DueDate { get; set; }

        public int? Mileage { get; set; }

        public string? LocationA { get; set; }   

        public long? LocationAId { get; set; }   

        public string? LocationB { get; set; }

        public long? LocationBId { get; set; }

        public double? Price { get; set; } //TODO: надо-ли?

        public string? Note { get; set; }

        public int CarCount { get; set; }

        public long MaterialId { get; set; }

        public long? ClientId { get; set; }

        public long GpId { get; set; }

        public ServiceType Service { get; set; }

        public Material Material { get; set; }

        public Client Client { get; set; }

        public ICollection<DriverTask> DriverTasks { get; set; }
    }
}
