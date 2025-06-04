namespace SFA.DAS.Reservations.Web.Models;

public class AutoSignOutViewModel(string providerPortalBaseUrl)
{
    public string ProviderPortalBaseUrl { get; set; } = providerPortalBaseUrl;
}