namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IExternalUrlHelper
    {
        string GenerateUrl(UrlParameters urlParameters);
        string GenerateAddApprenticeUrl(UrlParameters urlParameters);
    }
}
