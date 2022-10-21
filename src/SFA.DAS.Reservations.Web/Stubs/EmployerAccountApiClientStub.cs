﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.EAS.Account.Api.Client;
using SFA.DAS.EAS.Account.Api.Types;
using SFA.DAS.Encoding;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class EmployerAccountApiClientStub : IAccountApiClient
    {
        private readonly IEncodingService _encodingService;
        private string _userId;

        public EmployerAccountApiClientStub(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(string accountId)
        {
            return GetAccountUsers(1);
        }

        public Task<ICollection<TeamMemberViewModel>> GetAccountUsers(long accountId)
        {
            ICollection<TeamMemberViewModel> teamMembers = new List<TeamMemberViewModel>();

            teamMembers.Add(new TeamMemberViewModel
            {
                CanReceiveNotifications = true,
                Email = "stubby@stubsrus.com",
                Name = "Stubby McStubface",
                Role = "Owner",
                Status = InvitationStatus.Accepted,
                UserRef = _userId
            });

            return Task.FromResult(teamMembers);
        }

        public Task<ICollection<TransferConnectionViewModel>> GetTransferConnections(string accountHashedId)
        {
            const int fundingEmployerAccountId = 456;
            ICollection<TransferConnectionViewModel> transferConnections = new List<TransferConnectionViewModel>();

            transferConnections.Add(new TransferConnectionViewModel
            {
                FundingEmployerAccountId = fundingEmployerAccountId,
                FundingEmployerAccountName = "Stubs Funding",
                FundingEmployerHashedAccountId = _encodingService.Encode(fundingEmployerAccountId, EncodingType.AccountId),
                FundingEmployerPublicHashedAccountId = _encodingService.Encode(fundingEmployerAccountId, EncodingType.PublicAccountId)
            });

            return Task.FromResult(transferConnections);
        }

        public Task<ICollection<AccountDetailViewModel>> GetUserAccounts(string userId)
        {
            const int accountId = 123;
            
            _userId = userId;
            
            ICollection<AccountDetailViewModel> accountDetails = new List<AccountDetailViewModel>
            {
                new AccountDetailViewModel
                {
                    AccountId = accountId,
                    HashedAccountId = _encodingService.Encode(accountId, EncodingType.AccountId),
                    DasAccountName = "Stubby McStubface"
                }
            };

            return Task.FromResult(accountDetails);
        }

        public Task Ping()
        {
            throw new NotImplementedException();
        }

        public Task<ICollection<LegalEntityViewModel>> GetLegalEntityDetailsConnectedToAccount(string accountId)
        {
            throw new NotImplementedException();
        }

        #region not implemented
        public Task<AccountDetailViewModel> GetAccount(string hashedAccountId)
        {
            throw new NotImplementedException();
        }

        public Task<AccountDetailViewModel> GetAccount(long accountId)
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

        Task<T> IAccountApiClient.GetResource<T>(string uri)
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
        #endregion
    }
}