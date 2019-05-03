using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using SFA.DAS.Reservations.Infrastructure.Api;
using SFA.DAS.Reservations.Infrastructure.Configuration;
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
            List<GetReservationResponse> apiReservations,
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
        public async Task Then_Gets_Reservations_From_Api(
            long accountId,
            List<GetReservationResponse> apiReservations,
            [Frozen] ValidationResult validationResult,
            [Frozen] Mock<IOptions<ReservationsApiConfiguration>> mockOptions,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery {AccountId = accountId};
            validationResult.ValidationDictionary.Clear();

            await handler.Handle(query, CancellationToken.None);

            mockApiClient.Verify(client => client.GetAll<GetReservationResponse>(
                It.Is<IGetAllApiRequest>(request => 
                    request.GetAllUrl.StartsWith(mockOptions.Object.Value.Url) && 
                    request.GetAllUrl.Contains(query.AccountId.ToString()))), 
                Times.Once);
        }

        [Test, MoqAutoData]
        public async Task Then_Returns_Mapped_Reservations(
            long accountId,
            List<GetReservationResponse> apiReservations,
            [Frozen] ValidationResult validationResult,
            [Frozen] Mock<IApiClient> mockApiClient,
            GetReservationsQueryHandler handler)
        {
            var query = new GetReservationsQuery { AccountId = accountId };
            validationResult.ValidationDictionary.Clear();
            mockApiClient
                .Setup(client => client.GetAll<GetReservationResponse>(It.IsAny<IGetAllApiRequest>()))
                .ReturnsAsync(apiReservations);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(apiReservations);
        }
    }
}