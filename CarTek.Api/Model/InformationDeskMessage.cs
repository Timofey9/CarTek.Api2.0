namespace CarTek.Api.Model
{
    public enum InformationDeskMessageType
    {
        CartekUsers = 0,
        ExternalUsers = 1,
        All = 2
    }

    public class InformationDeskMessage
    {
        public long Id { get; set; }

        public DateTime DateCreated { get; set; }

        public string Message { get; set; }

        public InformationDeskMessageType MessageType { get; set; }
    }
}
