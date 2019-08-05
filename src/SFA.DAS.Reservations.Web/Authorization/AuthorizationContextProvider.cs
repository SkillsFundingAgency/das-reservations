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

            if (!cohortId.HasValue) return authorizationContext;

            var ukPrn = GetUkPrn();

            if (ukPrn.HasValue)
            {
                authorizationContext.AddCommitmentPermissionValues(cohortId.Value, Party.Provider, ukPrn.Value);
            }
            else
            {
                var accountId = GetAccountId();

                if (accountId.HasValue)
                {
                    authorizationContext.AddCommitmentPermissionValues(cohortId.Value, Party.Employer, accountId.Value);
                }
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

        private long? GetAccountId()
        {
            if (!_httpContextAccessor.HttpContext.GetRouteData().Values.TryGetValue("employerAccountId", out var hashedAccountId))
            {
                return null;
            }

            if (!_encodingService.TryDecode(hashedAccountId.ToString(), EncodingType.AccountId, out var accountId))
            {
                throw new UnauthorizedAccessException();
            }

            return accountId;
        }

        private long? GetCohortId()
        {
            if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue("cohortRef", out var cohortReferenceObj))
            {
                if (!_httpContextAccessor.HttpContext.Request.Query.TryGetValue("cohortReference", out cohortReferenceObj))
                {
                    return null;
                }
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
