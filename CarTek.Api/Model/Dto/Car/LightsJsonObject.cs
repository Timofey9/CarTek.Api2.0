namespace CarTek.Api.Model.Dto.Car
{
    public class LightsJsonObject
    {
        public bool? NearLight { get; set; }
        public bool BeamLight { get; set; }
        public bool? DistantLight { get; set; }
        public bool StopSignal { get; set; }
        public bool TurnSignal { get; set; }
    }
}
