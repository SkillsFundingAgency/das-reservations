using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.SearchReservations
{
    public class WhenSearchingReservations
    {
        [Test, MoqAutoData]
        public async Task And_Invalid_Then_Throws_Validation_Exception(
            SearchReservationsQuery query,
            ValidationResult validationResult,
            string propertyName,
            [Frozen]Mock<IValidator<SearchReservationsQuery>> mockValidator,
            [Frozen]Mock<IReservationService> mockReservationService,
            SearchReservationsQueryHandler handler)
        {
            validationResult.AddError(propertyName);
            mockValidator
                .Setup(validator => validator.ValidateAsync(query))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await handler.Handle(query, CancellationToken.None); };

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c => c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Search_Result_From_Service(
            SearchReservationsQuery query,
            [Frozen] ValidationResult validationResult,
            [Frozen]Mock<IReservationService> mockReservationService,
            SearchReservationsQueryHandler handler)
        {
            validationResult.ValidationDictionary.Clear();

            await handler.Handle(query, CancellationToken.None);

            mockReservationService.Verify(service => service.SearchReservations(
                query.ProviderId, 
                It.Is<SearchReservationsRequest>(request => request.SearchTerm == query.SearchTerm)));
        }

        [Test, MoqAutoData]
        public async Task Then_Maps_Reservations(
            SearchReservationsQuery query,
            [Frozen] ValidationResult validationResult,
            SearchReservationsResponse serviceResponse,
            [Frozen]Mock<IReservationService> mockReservationService,
            SearchReservationsQueryHandler handler)
        {
            validationResult.ValidationDictionary.Clear();
            mockReservationService
                .Setup(service => service.SearchReservations(
                    query.ProviderId, 
                    It.Is<SearchReservationsRequest>(request => request.SearchTerm == query.SearchTerm)))
                .ReturnsAsync(serviceResponse);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(serviceResponse.Reservations);
            result.NumberOfRecordsFound.Should().Be(serviceResponse.NumberOfRecordsFound);
        }
    }
}