namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        string GenerateUrl(string id = "", string controller = "", string action = "", string subDomain = "");
    }
}
