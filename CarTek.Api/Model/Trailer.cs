namespace CarTek.Api.Model
{
    #nullable disable
    public class Trailer
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        //В пути, на базе, в ремонте
        public string State { get; set; }

        public long? CarId { get; set; }

        public Car Car { get; set; }
    }
}
