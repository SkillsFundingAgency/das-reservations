using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservations
{
    [TestFixture]
    public class WhenGettingReservations
    {
        [Test, MoqAutoData]
        public void And_Invalid_Then_Throws_ValidationException(
            long accountId,
            string propertyName,
            ValidationResult validationResult,
            [Frozen] Mock<IValidator<GetReservationsQuery>> mockValidator,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery { AccountId = accountId };
            validationResult.AddError(propertyName);
            mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<GetReservationsQuery>()))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await handler.Handle(query, CancellationToken.None); };

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c => c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Reservations_From_Service(
            long accountId,
            [Frozen] ValidationResult validationResult,
            [Frozen] Mock<IReservationService> mockService,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery {AccountId = accountId};
            validationResult.ValidationDictionary.Clear();

            await handler.Handle(query, CancellationToken.None);

            mockService.Verify(client => client.GetReservations(accountId), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Mapped_Reservations(
            long accountId,
            List<Reservation> serviceReservations,
            [Frozen] ValidationResult validationResult,
            [Frozen] Mock<IReservationService> mockService,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery { AccountId = accountId };
            validationResult.ValidationDictionary.Clear();
            mockService
                .Setup(client => client.GetReservations(accountId))
                .ReturnsAsync(serviceReservations);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(serviceReservations);
        }
    }
}