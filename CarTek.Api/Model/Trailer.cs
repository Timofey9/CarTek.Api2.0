namespace CarTek.Api.Model
{
    #nullable disable
    public class Trailer
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        public int AxelsCount { get; set; }

        public long? CarId { get; set; }

        public Car Car { get; set; }
    }
}
