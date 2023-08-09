namespace CarTek.Api.Model
{
    public class Client
    {
        public long Id { get; set; }

        public string ClientName { get; set; }

        public string Inn { get; set; }

        public string Ogrn { get; set; }

        public string Kpp { get; set; }

        public string ClientAddress { get; set; }

        public ICollection<Order> Orders { get; set; }
    }

    public class CreateClientModel
    {
        public string ClientName { get; set; }

        public string Inn { get; set; }

        public string Ogrn { get; set; }

        public string Kpp { get; set; }

        public string ClientAddress { get; set; }
    }
}
