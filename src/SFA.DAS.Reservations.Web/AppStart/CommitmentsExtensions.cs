﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.Authorization.CommitmentPermissions.Client;
using SFA.DAS.Authorization.CommitmentPermissions.DependencyResolution;
using SFA.DAS.Authorization.CommitmentPermissions.DependencyResolution.Microsoft;
using SFA.DAS.Authorization.CommitmentPermissions.Handlers;
using SFA.DAS.Authorization.DependencyResolution;
using SFA.DAS.Authorization.DependencyResolution.Microsoft;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Stubs;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class CommitmentsExtensions
    {
        public static void AddCommitmentsApi(
            this IServiceCollection services, 
            IConfiguration configuration,
            IHostingEnvironment env)
        {
            if (env.IsDevelopment() && configuration.UseStub())
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

        public static void AddCommitmentsPermissionsApi(
            this IServiceCollection services,
            IConfiguration configuration, 
            IHostingEnvironment env)
        {
            if (env.IsDevelopment() && configuration.UseStub())
            {
                services.AddSingleton<ICommitmentPermissionsApiClient, CommitmentPermissionsApiStub>();
                services.AddAuthorizationHandler<AuthorizationHandler>();
            }
            else
            {
                services.AddCommitmentPermissionsAuthorization();
            }

        }
    }
}
