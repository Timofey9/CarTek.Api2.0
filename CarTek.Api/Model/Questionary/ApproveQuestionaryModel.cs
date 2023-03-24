namespace CarTek.Api.Model.Quetionary
{
    public class ApproveQuestionaryModel
    {
        public long DriverId { get; set; }
        public string DriverPass { get; set; }
        public string? AcceptanceComment { get; set; }
        public Guid QuestionaryId { get; set; }
    }
}
