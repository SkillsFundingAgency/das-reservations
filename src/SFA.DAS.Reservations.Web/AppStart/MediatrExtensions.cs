using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;
using SFA.DAS.Reservations.Application.FundingRules.Commands.MarkRuleAsRead;
using SFA.DAS.Reservations.Application.FundingRules.Queries.GetAccountFundingRules;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationCourse;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationStartDate;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservation;
using SFA.DAS.Reservations.Application.Reservations.Commands.CreateReservationLevyEmployer;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class MediatRExtensions
    {
        public static void AddMediatRValidation(this IServiceCollection services)
        {
            services.AddScoped(typeof(IValidator<CreateReservationCommand>), typeof(CreateReservationCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationCourseCommand>), typeof(CacheReservationCourseCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationStartDateCommand>), typeof(CacheReservationStartDateCommandValidator));
            services.AddScoped(typeof(IValidator<CacheReservationEmployerCommand>), typeof(CacheReservationEmployerCommandValidator));
            services.AddScoped(typeof(IValidator<IReservationQuery>), typeof(GetReservationQueryValidator));
            services.AddScoped(typeof(IValidator<CachedReservation>), typeof(CachedReservationValidator));
            services.AddScoped(typeof(IValidator<GetTrustedEmployersQuery>), typeof(GetTrustedEmployerQueryValidator));
            services.AddScoped(typeof(IValidator<GetReservationsQuery>), typeof(GetReservationsQueryValidator));
            services.AddScoped(typeof(IValidator<GetAvailableReservationsQuery>), typeof(GetAvailableReservationsQueryValidator));
            services.AddScoped(typeof(IValidator<MarkRuleAsReadCommand>), typeof(MarkRuleAsReadCommandValidator));
            services.AddScoped(typeof(IValidator<DeleteReservationCommand>), typeof(DeleteReservationCommandValidator));
            services.AddScoped(typeof(IValidator<GetAccountLegalEntityQuery>), typeof(GetAccountLegalEntityQueryValidator));
            services.AddScoped(typeof(IValidator<CreateReservationLevyEmployerCommand>), typeof(CreateReservationLevyEmployerCommandValidator));
            services.AddScoped(typeof(IValidator<GetProviderCacheReservationCommandQuery>), typeof(GetProviderCacheReservationCommandQueryValidator));
            services.AddScoped(typeof(IValidator<GetCohortQuery>), typeof(GetCohortQueryValidator));
            services.AddScoped(typeof(IValidator<GetAccountFundingRulesQuery>), typeof(GetAccountFundingRulesValidator));
            services.AddScoped(typeof(IValidator<SearchReservationsQuery>), typeof(SearchReservationsQueryValidator));
        }
    }
}
