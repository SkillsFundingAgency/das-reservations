using SFA.DAS.Provider.Shared.UI.Extensions;
using ProviderUrlParameters = SFA.DAS.Provider.Shared.UI.Models.UrlParameters;
using DomainUrlParameters = SFA.DAS.Reservations.Domain.Interfaces.UrlParameters;
using DomainUrlHelper = SFA.DAS.Reservations.Domain.Interfaces.IExternalUrlHelper;

namespace SFA.DAS.Reservations.Web.Infrastructure.ProviderSharedUi;

public class ProviderExternalUrlHelperAdapter : IExternalUrlHelper
{
    private readonly DomainUrlHelper _inner;

    public ProviderExternalUrlHelperAdapter(DomainUrlHelper inner)
    {
        _inner = inner;
    }

    public string GenerateUrl(ProviderUrlParameters urlParameters)
    {
        var mappedParameters = new DomainUrlParameters
        {
            Id = urlParameters.Id,
            Controller = urlParameters.Controller,
            Action = urlParameters.Action,
            SubDomain = urlParameters.SubDomain,
            Folder = urlParameters.Folder,
            QueryString = urlParameters.QueryString,
            RelativeRoute = urlParameters.RelativeRoute
        };

        return _inner.GenerateUrl(mappedParameters);
    }
}

