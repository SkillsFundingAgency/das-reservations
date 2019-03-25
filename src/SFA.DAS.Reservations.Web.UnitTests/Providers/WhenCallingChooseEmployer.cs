using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Queries;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    [TestFixture]
    public class WhenCallingChooseEmployer
    {
        [Test, MoqAutoData]
        public async Task Then_It_Calls_ProviderPermissions_Service_To_Get_Employers(
            uint ukPrn,
            IEnumerable<Employer> expectedEmployers,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller)
        {
            mockMediator
                .Setup(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = expectedEmployers});
   
            await controller.ChooseEmployer(ukPrn);

            mockMediator.Verify(m => m.Send(It.IsAny<GetTrustedEmployersQuery>(),  It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_It_Returns_The_Trusted_Employers(
            uint ukPrn,
            IEnumerable<Employer> expectedEmployers,
            [Frozen] Mock<IMediator> mockMediator,
            ProviderReservationsController controller)
        {
            mockMediator
                .Setup(service => service.Send(It.IsAny<GetTrustedEmployersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = expectedEmployers});

           
            var result =   await controller.ChooseEmployer(ukPrn);

            var viewModel = result.Should().BeOfType<ViewResult>()
                .Which.Model.Should().BeOfType<ChooseEmployerViewModel>()
                .Subject;
            
            viewModel.Employers.Should().BeEquivalentTo(expectedEmployers);
        }
    }
}