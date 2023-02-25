namespace CarTek.Api.Model.Dto.Car
{
    public class Wheel
    {
        public double Pressure { get; set; }
        //диск
        public bool RimState { get; set; }
        
        public bool PinsState { get; set; }

        public bool TireState { get; set; }
    }

    public class Axle
    {
        public Wheel LeftWheel { get; set; }
        public Wheel RightWheel { get; set; }
    }

    public class WheelsJson
    {
        public Axle FrontAxle { get; set; }
        public Axle BackAxle { get; set; }

        public Axle? MiddleAxle { get; set; }
    }
}
