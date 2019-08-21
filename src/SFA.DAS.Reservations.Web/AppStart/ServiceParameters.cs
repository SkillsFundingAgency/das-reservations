namespace SFA.DAS.Reservations.Web.AppStart
{
    public class ServiceParameters
    {
        public AuthenticationType? AuthenticationType { get; set; }
    }

    public enum AuthenticationType
    {
        Employer,
        Provider
    }
}