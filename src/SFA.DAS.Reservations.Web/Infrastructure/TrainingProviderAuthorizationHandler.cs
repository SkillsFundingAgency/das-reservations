using Microsoft.AspNetCore.Authorization;
using SFA.DAS.Reservations.Domain.Interfaces;
using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Web.Infrastructure
{
    /// <summary>
    /// Interface to define contracts related to Training Provider Authorization Handlers.
    /// </summary>
    public interface ITrainingProviderAuthorizationHandler
    {
        /// <summary>
        /// Contract to check is the logged in Provider is a valid Training Provider. 
        /// </summary>
        /// <param name="context">AuthorizationHandlerContext.</param>
        /// <returns>boolean.</returns>
        Task<bool> IsProviderAuthorized(AuthorizationHandlerContext context);
    }

    ///<inheritdoc cref="ITrainingProviderAuthorizationHandler"/>
    public class TrainingProviderAuthorizationHandler : ITrainingProviderAuthorizationHandler
    {
        private readonly IReservationsOuterService _outerApiService;

        public TrainingProviderAuthorizationHandler(
            IReservationsOuterService outerApiService)
        {
            _outerApiService = outerApiService;
        }

        public async Task<bool> IsProviderAuthorized(AuthorizationHandlerContext context)
        {
            var ukprn = GetProviderId(context);

            //if the ukprn is invalid return false.
            if (ukprn <= 0) return false;

            var providerStatusDetails = await _outerApiService.GetProviderStatus(ukprn);

            // Condition to check if the Provider Details has permission to access Apprenticeship Services based on the property value "CanAccessApprenticeshipService" set to True.
            return providerStatusDetails is { CanAccessService: true };
        }

        private static long GetProviderId(AuthorizationHandlerContext context)
        {
            return long.TryParse(context.User.FindFirst(c => c.Type.Equals(ProviderClaims.ProviderUkprn))?.Value, out var providerId)
                ? providerId
                : 0;
        }
    }
}
