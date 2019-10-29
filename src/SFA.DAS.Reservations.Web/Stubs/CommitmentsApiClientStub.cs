using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Domain.Commitments;
using SFA.DAS.Reservations.Domain.Commitments.Api;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Api;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class CommitmentsApiClientStub : IApiClient
    {
        private readonly IEncodingService _encodingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CommitmentsApiClientStub(IEncodingService encodingService, IHttpContextAccessor httpContextAccessor)
        {
            _encodingService = encodingService;
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<TResponse> Get<TResponse>(IGetApiRequest request)
        {
            var accountLegalEntityPublicHashedId = _httpContextAccessor.HttpContext.GetRouteValue("AccountLegalEntityPublicHashedId").ToString();
            var accountLegalEntityId = _encodingService.Decode(accountLegalEntityPublicHashedId, EncodingType.PublicAccountLegalEntityId);

            if (typeof(TResponse) == typeof(Cohort))
            {
                var cohortRequest = (GetCohortRequest) request;

                return Task.FromResult((TResponse) CreateCohort(cohortRequest.CohortId, accountLegalEntityId));
            }

            throw new NotImplementedException();
        }

        public Task<IEnumerable<TResponse>> GetAll<TResponse>(IGetAllApiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> Search<TResponse>(ISearchApiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<TResponse> Create<TResponse>(IPostApiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task Delete(IDeleteApiRequest request)
        {
            throw new NotImplementedException();
        }

        public Task<string> Ping()
        {
            throw new NotImplementedException();
        }

        private static object CreateCohort(long cohortId, long accountLegalEntityId)
        {
            return new Cohort
            {
                CohortId = cohortId,
                AccountLegalEntityId = accountLegalEntityId,
                IsFundedByTransfer = false,
                LegalEntityName = "Test Legal Entity",
                ProviderName = "Test Provider",
                WithParty = CohortParty.Employer
            };
        }
    }
}
