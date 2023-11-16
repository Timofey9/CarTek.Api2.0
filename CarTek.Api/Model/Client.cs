namespace CarTek.Api.Model
{
    public class Client
    {
        public long Id { get; set; }

        public string ClientName { get; set; }

        public string Inn { get; set; }

        public string ClientAddress { get; set; }

        public Unit ClientUnit { get; set; }

        public double? FixedPrice { get; set; }

        public ICollection<Order> Orders { get; set; }
    }

    public class CreateClientModel
    {
        public long? Id { get; set; }
        public string ClientName { get; set; }
        public Unit ClientUnit { get; set; }
        public string Inn { get; set; }
        public double? FixedPrice { get; set; }
        public string ClientAddress { get; set; }
    }
}
