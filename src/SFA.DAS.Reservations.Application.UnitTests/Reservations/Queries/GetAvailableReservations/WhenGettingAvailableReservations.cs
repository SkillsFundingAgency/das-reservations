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
using SFA.DAS.Reservations.Application.Reservations.Queries.GetAvailableReservations;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetAvailableReservations
{
    [TestFixture]
    public class WhenGettingAvailableReservations
    {
        [Test, MoqAutoData]
        public void And_Invalid_Then_Throws_ValidationException(
            long accountId,
            string propertyName,
            ValidationResult validationResult,
            [Frozen] Mock<IValidator<GetAvailableReservationsQuery>> mockValidator,
            GetAvailableReservationsQueryHandler handler)
        {
            var query = new GetAvailableReservationsQuery { AccountId = accountId };
            validationResult.AddError(propertyName);
            mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<GetAvailableReservationsQuery>()))
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
            GetAvailableReservationsQueryHandler handler)
        {
            var query = new GetAvailableReservationsQuery {AccountId = accountId};
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
            GetAvailableReservationsQueryHandler handler)
        {
            var query = new GetAvailableReservationsQuery { AccountId = accountId };
            validationResult.ValidationDictionary.Clear();
            serviceReservations.ForEach(reservation => reservation.Status = ReservationStatus.Pending);
            mockService
                .Setup(client => client.GetReservations(accountId))
                .ReturnsAsync(serviceReservations);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(serviceReservations);
        }

        [Test, MoqAutoData]
        public async Task And_Status_Not_Pending_Then_Does_Not_Return_Reservation(
            long accountId,
            List<Reservation> serviceReservations,
            [Frozen] ValidationResult validationResult,
            [Frozen] Mock<IReservationService> mockService,
            GetAvailableReservationsQueryHandler handler)
        {
            var query = new GetAvailableReservationsQuery { AccountId = accountId };
            validationResult.ValidationDictionary.Clear();
            serviceReservations[0].Status = ReservationStatus.Completed;
            mockService
                .Setup(client => client.GetReservations(accountId))
                .ReturnsAsync(serviceReservations);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().NotContain(serviceReservations[0]);
        }
    }
}