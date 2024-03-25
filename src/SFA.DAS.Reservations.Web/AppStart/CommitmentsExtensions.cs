using Microsoft.Extensions.DependencyInjection;
using SFA.DAS.Reservations.Application.Commitments.Services;

namespace SFA.DAS.Reservations.Web.AppStart;

public static class CommitmentsExtensions
{
    public static void AddCommitmentsApi(this IServiceCollection services)
    {
        services.AddTransient<ICommitmentService, CommitmentService>();
    }
}
