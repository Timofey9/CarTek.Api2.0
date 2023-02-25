namespace CarTek.Api.Model.Dto
{
    public class QuestionaryModel
    {
        public long Id { get; set; }

        public Guid UniqueId { get; set; }

        public string ImagesPath { get; set; }

        public bool IsOk { get; set; }

        public DateTime LastUpdated { get; set; }

        public string Comment { get; set; }

        public bool WasApproved { get; set; }

        //Пробег
        public int Mileage { get; set; }

        //Nav properties
        public CarModel Car { get; set; }
        public DriverModel Driver { get; set; }

        public long UserId { get; set; }
        public UserModel UpdatedBy { get; set; }
    }
}
