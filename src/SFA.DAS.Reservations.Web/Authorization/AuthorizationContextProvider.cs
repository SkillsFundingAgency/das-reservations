using System;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Authorization.CommitmentPermissions.Context;
using SFA.DAS.Authorization.Context;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.Reservations.Web.Authorization
{
    public class AuthorizationContextProvider : IAuthorizationContextProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncodingService _encodingService;

        public AuthorizationContextProvider(IHttpContextAccessor httpContextAccessor, IEncodingService encodingService)
        {
            _httpContextAccessor = httpContextAccessor;
            _encodingService = encodingService;
        }

        public IAuthorizationContext GetAuthorizationContext()
        {
            var authorizationContext = new AuthorizationContext();

            var cohortId = GetCohortId();
            var ukPrn = GetUkPrn();

            if (cohortId.HasValue && ukPrn.HasValue)
            {
                authorizationContext.AddCommitmentPermissionValues(cohortId.Value, Party.Provider, ukPrn.Value);
            }

            return authorizationContext;
        }

        private long? GetUkPrn()
        {
            if (!_httpContextAccessor.HttpContext.GetRouteData().Values.TryGetValue("ukprn", out var ukPrnObj))
            {
                return null;
            }

            if (!long.TryParse(ukPrnObj.ToString(), out var ukPrn))
            {
                throw new UnauthorizedAccessException();
            }

            return ukPrn;
        }

        private long? GetCohortId()
        {
            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue("cohortRef", out var cohortReferenceObj))
            {
                return null;
            }

            if (!cohortReferenceObj.Any())
            {
                return null;
            }

            var cohortReference = cohortReferenceObj.First();

            if (string.IsNullOrEmpty(cohortReference))
            {
                return null;
            }

            if (!_encodingService.TryDecode(cohortReference, EncodingType.CohortReference, out var cohortId))
            {
                throw new UnauthorizedAccessException();
            }

            return cohortId;
        }
    }
}
