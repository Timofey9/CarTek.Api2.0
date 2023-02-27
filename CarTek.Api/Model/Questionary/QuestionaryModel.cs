namespace CarTek.Api.Model.Dto
{
    public class QuestionaryModel
    {
        public long Id { get; set; }

        public Guid UniqueId { get; set; }

        public string ImagesPath { get; set; }

        public string CarQuestionaryModel { get; set; }

        public string TrailerQuestionaryModel { get; set; }

        public TransportState State { get; set; }

        public DateTime LastUpdated { get; set; }

        public string Comment { get; set; }

        public bool WasApproved { get; set; }

        //Пробег
        public int Mileage { get; set; }

        //Nav properties
        public CarModel Car { get; set; }
        public DriverModel Driver { get; set; }
        public UserModel User { get; set; }
    }
}
