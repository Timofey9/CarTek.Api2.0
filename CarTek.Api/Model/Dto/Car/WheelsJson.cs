namespace CarTek.Api.Model.Dto.Car
{
    public class Wheel
    {
        private string _pressure;

        public string Pressure {
            get
            {
                return _pressure;
            }
            set
            {
                var isDouble = double.TryParse(value, out var result);

                if (isDouble)
                {
                    if(result > 8 && result < 11)
                    {
                        _pressure = "true";
                    }
                    else
                    {
                        _pressure = "false";
                    }
                }
                else
                {
                    _pressure = value;
                }
            }
        }
        //диск
        public bool RimState { get; set; }

        public bool PinsState { get; set; }

        public bool TireState { get; set; }
    }

    public class Axle
    {
        public Wheel LeftWheel { get; set; }

        public Wheel? LeftWheel2 { get; set; }

        public Wheel RightWheel { get; set; }

        public Wheel? RightWheel2 { get; set; }
    }

    public class WheelsJson
    {
        public Axle FrontAxle { get; set; }
        public Axle BackAxle { get; set; }
        public Axle? MiddleAxle { get; set; }
    }
}
