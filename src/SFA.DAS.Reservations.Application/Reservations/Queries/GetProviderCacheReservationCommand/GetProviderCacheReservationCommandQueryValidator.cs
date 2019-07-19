using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class GetProviderCacheReservationCommandQueryValidator : IValidator<GetProviderCacheReservationCommandQuery>
    {
        public Task<ValidationResult> ValidateAsync(GetProviderCacheReservationCommandQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
