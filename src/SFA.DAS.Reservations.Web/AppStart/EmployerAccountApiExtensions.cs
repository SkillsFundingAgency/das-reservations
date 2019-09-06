using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Infrastructure.Services;
using AccountApiConfiguration = SFA.DAS.Reservations.Infrastructure.Configuration.AccountApiConfiguration;

namespace SFA.DAS.Reservations.Web.AppStart
{
    public static class EmployerAccountApiExtensions
    {
        public static void AddEmployerAccountApi(
            this IServiceCollection services, 
            IConfiguration configuration,
            IHostingEnvironment env)
        {
            var serviceProvider = services.BuildServiceProvider();
            var config = serviceProvider.GetService<IOptions<AccountApiConfiguration>>().Value;

            if (env.IsDevelopment() && config.UseStub)
            {
                services.AddScoped<IAccountApiClient, EmployerAccountApiStub>();
            }
            else
            {
                services.AddSingleton<IAccountApiConfiguration, AccountApiConfiguration>(cfg => cfg.GetService<IOptions<AccountApiConfiguration>>().Value);
                services.AddTransient<IAccountApiClient, AccountApiClient>();
                services.AddTransient<IEmployerAccountService, EmployerAccountService>();
            }
        }
    }

    public class EmployerAccountApiStub : IAccountApiClient
    {
        public Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<AccountDetailViewModel> GetAccount(long accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            throw new NotImplementedException();
        }

        public Task<EmployerAgreementView> GetEmployerAgreement(string accountId, string legalEntityId, string agreementId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ResourceViewModel>> GetLegalEntitiesConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<LegalEntityViewModel> GetLegalEntity(string accountId, long id)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<LevyDeclarationViewModel>> GetLevyDeclarations(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<PagedApiResponseViewModel<AccountLegalEntityViewModel>> GetPageOfAccountLegalEntities(int pageNumber = 1, int pageSize = 1000)
        {
            throw new NotImplementedException();
        }

        public Task<PagedApiResponseViewModel<AccountWithBalanceViewModel>> GetPageOfAccounts(int pageNumber = 1, int pageSize = 1000, DateTime? toDate = null)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<ResourceViewModel>> GetPayeSchemesConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<T> GetResource<T>(string uri) where T : IAccountResource
        {
            throw new NotImplementedException();
        }

        public Task<StatisticsViewModel> GetStatistics()
        {
            throw new NotImplementedException();
        }

        public Task<TransactionsViewModel> GetTransactions(string accountId, int year, int month)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TransactionSummaryViewModel>> GetTransactionSummary(string accountId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<TransferConnectionViewModel>> GetTransferConnections(string accountHashedId)
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        {
            throw new NotImplementedException();
        }
    }
}