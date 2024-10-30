using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Commands.DeleteReservation;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.DeleteReservation
{
    [TestFixture]
    public class WhenDeletingAReservation
    {
        [Test, MoqAutoData]
        public async Task Then_Validates_The_Command(
            DeleteReservationCommand command,
            [Frozen] Mock<IValidator<DeleteReservationCommand>> mockValidator,
            DeleteReservationCommandHandler handler)
        {
            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

            await handler.Handle(command, CancellationToken.None);

            mockValidator
                .Verify(validator => validator.ValidateAsync(command), 
                    Times.Once);
        }

        [Test, MoqAutoData]
        public async Task And_Command_Not_Valid_Then_Throws_ValidationException(
            DeleteReservationCommand command,
            string propertyName,
            ValidationResult validationResult,
            [Frozen] Mock<IValidator<DeleteReservationCommand>> mockValidator,
            DeleteReservationCommandHandler handler)
        {
            validationResult.AddError(propertyName);
            mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<DeleteReservationCommand>()))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await handler.Handle(command, CancellationToken.None); };

            (await act.Should().ThrowExactlyAsync<ValidationException>())
                .Which.ValidationResult.MemberNames.First(c => c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, MoqAutoData]
        public async Task Then_Sends_Delete_Request_To_Reservations_Api(
            DeleteReservationCommand command,
            [Frozen] Mock<IValidator<DeleteReservationCommand>> mockValidator,
            [Frozen] Mock<IApiClient> mockApiClient,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            DeleteReservationCommandHandler handler)
        {
            var expectedApiRequest = new ReservationApiRequest(mockOptions.Object.Value.Url, command.ReservationId, command.DeletedByEmployer);
            mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult());

            await handler.Handle(command, CancellationToken.None);

            mockApiClient
                .Verify(client => client.Delete(
                        It.Is<IDeleteApiRequest>(request => request.DeleteUrl == expectedApiRequest.DeleteUrl)), 
                    Times.Once);
        }
    }
}