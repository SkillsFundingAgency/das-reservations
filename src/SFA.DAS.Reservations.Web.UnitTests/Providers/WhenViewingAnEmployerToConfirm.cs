using System;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Providers
{
    public class WhenViewingAnEmployerToConfirm
    {
        
        [Test, MoqAutoData]
        public async Task Then_The_ViewModel_Is_Passed_To_The_View(
            [Frozen] Mock<IMediator> mediator,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel)
        {
            //Arrange
            viewModel.Id = null;

            //Act
            var actual = await controller.ConfirmEmployer(viewModel);

            //Assert
            Assert.IsNotNull(actual);
            var viewResult = actual as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ConfirmEmployerViewModel;
            model.Should().BeEquivalentTo(viewModel);
            mediator.Verify(x=>x.Send(It.IsAny<GetCachedReservationQuery>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_If_The_ViewModel_Is_Null_It_Is_Read_From_The_Cache_And_Account_Id_Is_Hashed(
            string hashedAccountId,
            [Frozen] Mock<IMediator> mediator,
            [Frozen] Mock<IEncodingService> encodingService,
            ProviderReservationsController controller,
            ConfirmEmployerViewModel viewModel,
            GetCachedReservationResult cachedResult)
        {
            //Arrange
            mediator
                .Setup(x => x.Send(
                    It.Is<GetCachedReservationQuery>(c => c.Id.Equals(viewModel.Id) && c.UkPrn.Equals(viewModel.UkPrn)),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(cachedResult);
            encodingService.Setup(x => x.Encode(cachedResult.AccountId, EncodingType.AccountId)).Returns(hashedAccountId);

            //Act
            var actual = await controller.ConfirmEmployer(new ConfirmEmployerViewModel{Id=viewModel.Id,UkPrn = viewModel.UkPrn});

            //Assert
            Assert.IsNotNull(actual);
            var viewResult = actual as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ConfirmEmployerViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(cachedResult.AccountLegalEntityName, model.AccountLegalEntityName);
            Assert.AreEqual(hashedAccountId, model.AccountPublicHashedId);
            Assert.AreEqual(cachedResult.AccountLegalEntityPublicHashedId, model.AccountLegalEntityPublicHashedId);
            Assert.AreEqual(viewModel.UkPrn, model.UkPrn);
        }
    }
}
