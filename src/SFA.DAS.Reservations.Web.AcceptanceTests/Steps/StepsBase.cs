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
        protected const long AccountLegalEntityId = 1;
        protected const uint ProviderId = 15214;
        protected Guid ReservationId;
        protected readonly IServiceProvider Services;
        protected readonly TestData TestData;
        
        protected long SelectedAccountId;
        protected string SelectedHashedAccountId;

        public StepsBase(TestServiceProvider serviceProvider, TestData testData)
        {
            Services = serviceProvider;
            TestData = testData;
            ReservationId = Guid.NewGuid();
            SelectedAccountId = TestDataValues.NonLevyAccountId;
            SelectedHashedAccountId = TestDataValues.NonLevyHashedAccountId;
        }

        [BeforeScenario()]
        public void InitialiseTestData()
        {
            SetTestData();

            ArrangeApiClient();
        }

        private void ArrangeApiClient()
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

            mock.Setup(x =>
                    x.Create<CreateReservationResponse>(
                        It.Is<ReservationApiRequest>(c => c.Id.Equals(TestData.ReservationRouteModel.Id))))
                .ReturnsAsync(new CreateReservationResponse { Id = TestData.ReservationRouteModel.Id.Value });

            mock.Setup(x =>
                    x.Create<CreateReservationResponse>(
                        It.Is<ReservationApiRequest>(c => !c.Id.Equals(TestData.ReservationRouteModel.Id))))
                .ReturnsAsync(new CreateReservationResponse { Id = Guid.NewGuid() });

            mock.Setup(x => x.Get<GetCoursesApiResponse>(It.IsAny<GetCoursesApiRequest>())).ReturnsAsync(
                new GetCoursesApiResponse
                {
                    Courses = new List<Course> {TestData.Course}
                });

            mock.Setup(x => x.GetAll<GetReservationResponse>(
                It.IsAny<ReservationApiRequest>())).ReturnsAsync(TestData.Reservations);
        }


        protected void SetTestData()
        {
            TestData.ReservationRouteModel = new ReservationsRouteModel
            {
                EmployerAccountId = SelectedHashedAccountId,
                AccountLegalEntityPublicHashedId = "ABC123",
                Id = ReservationId
            };

            TestData.AccountLegalEntity = new AccountLegalEntity
            {
                AccountId = SelectedAccountId,
                AccountLegalEntityId = 1,
                AccountLegalEntityPublicHashedId = "ABC123",
                AccountLegalEntityName = "Test Legal Entity",
                AgreementType = AgreementType.NonLevyExpressionOfInterest,
                IsLevy = SelectedAccountId.Equals(TestDataValues.LevyAccountId),
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
