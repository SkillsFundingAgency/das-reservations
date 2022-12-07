using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenViewingTheEmployerAgreementNotSigned
    {
        [Test, MoqAutoData]
        public async Task Then_The_ViewModel_Is_Populated_From_The_Query_And_Passed_To_The_View(
            string accountLegalEntityId,
            string dashboardUrl,
            GetAccountLegalEntityResult result,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IMediator> mediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ProviderReservationsController controller,
            EmployerAgreementNotSignedViewModel viewModel)
        {
            //Arrange
            result.LegalEntity.AgreementSigned = false;
            externalUrlHelper.Setup(x => x.GenerateDashboardUrl(null)).Returns(dashboardUrl);
            mediator.Setup(x =>
                x.Send(
                    It.Is<GetAccountLegalEntityQuery>(c =>
                        c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityId)),
                    It.IsAny<CancellationToken>())).ReturnsAsync(result);

            //Act
            var actual = await controller.EmployerAgreementNotSigned(routeModel, accountLegalEntityId);

            //Assert
            Assert.IsNotNull(actual);
            var viewResult = actual as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as EmployerAgreementNotSignedViewModel;
            Assert.IsNotNull(model);
            model.AccountName.Should().Be(result.LegalEntity.AccountLegalEntityName);
            model.DashboardUrl.Should().Be(dashboardUrl);
            mediator.Verify(x=>x.Send(It.IsAny<GetAccountLegalEntityQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_Id_Is_Null_Or_Empty_Then_The_RouteModel_Is_Checked_For_The_Id(
            string accountLegalEntityId,
            string dashboardUrl,
            GetAccountLegalEntityResult result,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IMediator> mediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ProviderReservationsController controller,
            EmployerAgreementNotSignedViewModel viewModel)
        {
            //Arrange
            result.LegalEntity.AgreementSigned = false;
            externalUrlHelper.Setup(x => x.GenerateDashboardUrl(null)).Returns(dashboardUrl);
            mediator.Setup(x =>
                x.Send(
                    It.Is<GetAccountLegalEntityQuery>(c =>
                        c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityId)),
                    It.IsAny<CancellationToken>())).ReturnsAsync(result);
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntityId;

            //Act
            var actual = await controller.EmployerAgreementNotSigned(routeModel, "");
            
            //Assert
            Assert.IsNotNull(actual);
            var viewResult = actual as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as EmployerAgreementNotSignedViewModel;
            Assert.IsNotNull(model);
            model.AccountName.Should().Be(result.LegalEntity.AccountLegalEntityName);
            model.DashboardUrl.Should().Be(dashboardUrl);
            mediator.Verify(x=>x.Send(It.IsAny<GetAccountLegalEntityQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_User_Has_Come_From_Select_Reservation_The_BackUrl_Is_Populated(
            string accountLegalEntityId,
            string dashboardUrl,
            string previousPageUrl,
            GetAccountLegalEntityResult result,
            [Frozen] Mock<IExternalUrlHelper> externalUrlHelper,
            [Frozen] Mock<IMediator> mediator,
            ReservationsRouteModel routeModel,
            [NoAutoProperties] ProviderReservationsController controller,
            EmployerAgreementNotSignedViewModel viewModel)
        {
            //Arrange
            result.LegalEntity.AgreementSigned = false;
            externalUrlHelper.Setup(x => x.GenerateDashboardUrl(null)).Returns(dashboardUrl);
            mediator.Setup(x =>
                x.Send(
                    It.Is<GetAccountLegalEntityQuery>(c =>
                        c.AccountLegalEntityPublicHashedId.Equals(accountLegalEntityId)),
                    It.IsAny<CancellationToken>())).ReturnsAsync(result);
            routeModel.AccountLegalEntityPublicHashedId = accountLegalEntityId;
            routeModel.IsFromSelect = true;
            routeModel.PreviousPage = previousPageUrl;

            //Act
            var actual = await controller.EmployerAgreementNotSigned(routeModel, "");
            
            //Assert
            Assert.IsNotNull(actual);
            var viewResult = actual as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as EmployerAgreementNotSignedViewModel;
            Assert.IsNotNull(model);
            model.AccountName.Should().Be(result.LegalEntity.AccountLegalEntityName);
            model.BackUrl.Should().Be(previousPageUrl);
        }
    }
}