using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Stubs;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class CommitmentsExtensions
    {
        public static void AddCommitmentsApi(this IServiceCollection services, IConfiguration configuration,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                services.AddSingleton<CommitmentsApiClientStub>();

                services.AddTransient<ICommitmentService, CommitmentService>(provider => new CommitmentService(
                    provider.GetService<CommitmentsApiClientStub>(),
                    provider.GetService<IOptions<CommitmentsApiConfiguration>>()));
            }
            else
            {
                services.AddSingleton<CommitmentsApiClient>();

                services.AddTransient<ICommitmentService, CommitmentService>(provider => new CommitmentService(
                    provider.GetService<CommitmentsApiClient>(),
                    provider.GetService<IOptions<CommitmentsApiConfiguration>>()));
            }
        }
    }
}
