using System.Collections.Generic;
using System.Linq;
using api = SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Web.Models
{
    public class SelectLegalEntityViewModel
    {
        public SelectLegalEntityViewModel(
            ReservationsRouteModel routeModel,
            IEnumerable<api.LegalEntityViewModel> apiLegalEntities)
        {
            RouteModel = routeModel;
            LegalEntities = apiLegalEntities?.Select(apiModel => new LegalEntityViewModel(apiModel));
        }

        public IEnumerable<LegalEntityViewModel> LegalEntities { get; }
        public ReservationsRouteModel RouteModel { get; }
        public string LegalEntity { get; set; }
    }

    public class LegalEntityViewModel
    {
        public LegalEntityViewModel(api.LegalEntityViewModel apiModel)
        {
            Name = apiModel.Name;
            AccountLegalEntityPublicHashedId = apiModel.AccountLegalEntityPublicHashedId;
        }

        public string Name { get; }
        public string AccountLegalEntityPublicHashedId { get; }
    }
}