namespace CarTek.Api.Model.Dto
{
    public class TrailerModel
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }
        public int AxelsCount { get; set; }

        //В пути, на базе, в ремонте
        public string State { get; set; }

        public long? CarId { get; set; }
    }
}
