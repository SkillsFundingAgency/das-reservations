using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SFA.DAS.Common.Domain.Types;
using SFA.DAS.Reservations.Domain.Courses;
using SFA.DAS.Reservations.Domain.Courses.Api;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Employers.Api;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Web.AcceptanceTests.Infrastructure;
using SFA.DAS.Reservations.Web.Models;
using TechTalk.SpecFlow;

namespace SFA.DAS.Reservations.Web.AcceptanceTests.Steps
{
    public class StepsBase
    {
        protected readonly IServiceProvider Services;
        protected readonly TestData TestData;
        
        public StepsBase(TestServiceProvider serviceProvider, TestData testData)
        {
            Services = serviceProvider;
            TestData = testData;
        }

        protected void ArrangeApiClient()
        {
            var apiClient = Services.GetService<IApiClient>();
            var mock = Mock.Get(apiClient);

            SetupSharedApiClientResponses(mock);

            SetupNonLevyApiClientResponses(mock);

            SetupLevyApiClientResponses(mock);
        }

        private void SetupLevyApiClientResponses(Mock<IApiClient> mock)
        {
            mock.Setup(x => x.Get<AccountReservationStatusResponse>(It.Is<AccountReservationStatusRequest>(c => c.AccountId.Equals(TestDataValues.LevyAccountId))))
                .ReturnsAsync(new AccountReservationStatusResponse
                    { CanAutoCreateReservations = true });
        }

        private void SetupNonLevyApiClientResponses(Mock<IApiClient> mock)
        {
            mock.Setup(x => x.Get<AccountReservationStatusResponse>(It.Is<AccountReservationStatusRequest>(c => c.AccountId.Equals(TestDataValues.NonLevyAccountId))))
                .ReturnsAsync(new AccountReservationStatusResponse
                    { CanAutoCreateReservations = false });
        }

        private void SetupSharedApiClientResponses(Mock<IApiClient> mock)
        {
            mock.Setup(x => x.GetAll<AccountLegalEntity>(It.IsAny<GetAccountLegalEntitiesRequest>()))
                .ReturnsAsync(new List<AccountLegalEntity> {TestData.AccountLegalEntity});
            mock.Setup(x => x.Get<GetAccountFundingRulesApiResponse>(It.IsAny<GetAccountFundingRulesApiRequest>()))
                .ReturnsAsync(new GetAccountFundingRulesApiResponse {GlobalRules = new List<GlobalRule>()});
            mock.Setup(x => x.Get<GetFundingRulesApiResponse>(It.IsAny<GetFundingRulesApiRequest>()))
                .ReturnsAsync(new GetFundingRulesApiResponse {GlobalRules = new List<GlobalRule>()});

            mock.Setup(x =>x.Create<CreateReservationResponse>(It.IsAny<ReservationApiRequest>()))
                .ReturnsAsync((ReservationApiRequest request) => new CreateReservationResponse { Id = request.Id });

            mock.Setup(x => x.Get<GetCoursesApiResponse>(It.IsAny<GetCoursesApiRequest>())).ReturnsAsync(
                new GetCoursesApiResponse
                {
                    Courses = new List<Course> {TestData.Course}
                });

            mock.Setup(x => x.GetAll<GetReservationResponse>(
                It.IsAny<ReservationApiRequest>())).ReturnsAsync(TestData.Reservations);

            mock.Setup(x => x.Get<GetAvailableDatesApiResponse>(
                    It.Is<GetAvailableDatesApiRequest>(
                        c => c.AccountLegalEntityId.Equals(TestData.AccountLegalEntity.AccountLegalEntityId))))
                .ReturnsAsync(
                    new GetAvailableDatesApiResponse
                    {
                        AvailableDates = new List<TrainingDateModel>
                        {
                            new TrainingDateModel
                            {
                                StartDate = DateTime.UtcNow.AddMonths(1),
                                EndDate = DateTime.UtcNow.AddMonths(3)
                            }
                        }
                    });
            mock.Setup(x =>
                    x.GetAll<AccountLegalEntity>(
                        It.Is<GetTrustedEmployersRequest>(c => c.Id.Equals(TestData.ReservationRouteModel.UkPrn))))
                .ReturnsAsync(new List<AccountLegalEntity>
                {
                    new AccountLegalEntity
                    {
                        AccountId = TestDataValues.NonLevyAccountId,
                        AccountName = "Account 1",
                        AccountLegalEntityId = TestDataValues.NonLevyAccountLegalEntityId,
                        AccountLegalEntityName = "Legal Entity 1",
                        AgreementSigned = true,
                        IsLevy = false,
                        AgreementType = AgreementType.NonLevyExpressionOfInterest
                    }
                });
        }
        
        protected void SetupNonLevyEmployerTestData()
        {
            TestData.UserId = Guid.NewGuid();

            TestData.ReservationRouteModel = new ReservationsRouteModel
            {
                EmployerAccountId = TestDataValues.NonLevyHashedAccountId,
                AccountLegalEntityPublicHashedId = TestDataValues.NonLevyHashedAccountLegalEntityId
            };

            TestData.AccountLegalEntity = new AccountLegalEntity
            {
                AccountId = TestDataValues.NonLevyAccountId,
                AccountLegalEntityId = TestDataValues.NonLevyAccountLegalEntityId,
                AccountLegalEntityPublicHashedId = TestDataValues.NonLevyHashedAccountLegalEntityId,
                AccountLegalEntityName = "Test Legal Entity",
                AgreementType = AgreementType.NonLevyExpressionOfInterest,
                IsLevy = false,
                LegalEntityId = 1,
                ReservationLimit = 5
            };
            TestData.Course = new Course("1", "Test Course", 1);

            TestData.TrainingDate = new TrainingDateModel
            {
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(3)
            };
            
            TestData.Reservations = new List<GetReservationResponse>();
        }

        protected void SetupLevyEmployerTestData()
        {
            TestData.UserId = Guid.NewGuid();

            TestData.ReservationRouteModel = new ReservationsRouteModel
            {
                EmployerAccountId = TestDataValues.LevyHashedAccountId,
                AccountLegalEntityPublicHashedId = TestDataValues.LevyHashedAccountLegalEntityId
            };

            TestData.AccountLegalEntity = new AccountLegalEntity
            {
                AccountId = TestDataValues.LevyAccountId,
                AccountLegalEntityId = TestDataValues.LevyAccountLegalEntityId,
                AccountLegalEntityPublicHashedId = TestDataValues.LevyHashedAccountLegalEntityId,
                
                AccountLegalEntityName = "Test Legal Entity",
                AgreementType = AgreementType.Levy,
                IsLevy = true,
                LegalEntityId = 1,
                ReservationLimit = 5
            };
            TestData.Course = new Course("1", "Test Course", 1);

            TestData.TrainingDate = new TrainingDateModel
            {
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(3)
            };
            
            TestData.Reservations = new List<GetReservationResponse>();
        }

        protected void SetupProviderTestData()
        {
            TestData.UserId = Guid.NewGuid();

            TestData.ReservationRouteModel = new ReservationsRouteModel
            {
                UkPrn = 10003456
            };

            TestData.AccountLegalEntity = new AccountLegalEntity
            {
                AccountId = TestDataValues.NonLevyAccountId,
                AccountLegalEntityId = TestDataValues.NonLevyAccountLegalEntityId,
                AccountLegalEntityPublicHashedId = TestDataValues.NonLevyHashedAccountLegalEntityId,
                AccountLegalEntityName = "Test Legal Entity",
                AgreementType = AgreementType.NonLevyExpressionOfInterest,
                IsLevy = false,
                LegalEntityId = 1,
                ReservationLimit = 5
            };
            TestData.Course = new Course("1", "Test Course", 1);

            TestData.TrainingDate = new TrainingDateModel
            {
                StartDate = DateTime.UtcNow.AddMonths(1),
                EndDate = DateTime.UtcNow.AddMonths(3)
            };
            
            TestData.Reservations = new List<GetReservationResponse>();
        }
    }
}
