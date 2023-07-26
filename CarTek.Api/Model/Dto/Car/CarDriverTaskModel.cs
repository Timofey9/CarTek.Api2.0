namespace CarTek.Api.Model.Dto.Car
{
    public class CarDriverTaskModel
    {
        public long Id { get; set; }

        public string Plate { get; set; }

        public string Brand { get; set; }

        public string Model { get; set; }

        //Должно быть максимум 2
        public ICollection<DriverTaskCarModel> DriverTasks { get; set; }
    }
}
