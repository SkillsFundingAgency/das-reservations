using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
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
        public void And_Invalid_Then_Throws_Validation_Exception(
            SearchReservationsQuery query,
            ValidationResult validationResult,
            string propertyName,
            [Frozen]Mock<IValidator<SearchReservationsQuery>> mockValidator,
            SearchReservationsQueryHandler handler)
        {
            validationResult.AddError(propertyName);
            validationResult.FailedAuthorisationValidation = false;
            mockValidator
                .Setup(validator => validator.ValidateAsync(query))
                .ReturnsAsync(validationResult);

            Func<Task> act = async () => { await handler.Handle(query, CancellationToken.None); };

            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c => c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, MoqAutoData]
        public void And_Has_No_Permissions_Then_Throws_No_Permissions_Exception(SearchReservationsQuery query,
            ValidationResult validationResult,
            string propertyName,
            [Frozen]Mock<IValidator<SearchReservationsQuery>> mockValidator,
            SearchReservationsQueryHandler handler)
        {
            validationResult.ValidationDictionary.Clear();
            validationResult.FailedAuthorisationValidation = true;
            mockValidator
                .Setup(validator => validator.ValidateAsync(query))
                .ReturnsAsync(validationResult);

            Assert.ThrowsAsync<ProviderNotAuthorisedException>(() => handler.Handle(query, CancellationToken.None));
        }

        [Test, MoqAutoData]
        public async Task Then_Gets_Search_Result_From_Service(
            SearchReservationsQuery query,
            [Frozen] ValidationResult validationResult,
            [Frozen]Mock<IReservationService> mockReservationService,
            SearchReservationsQueryHandler handler)
        {
            validationResult.ValidationDictionary.Clear();
            validationResult.FailedAuthorisationValidation = false;

            await handler.Handle(query, CancellationToken.None);

            mockReservationService.Verify(service => service.SearchReservations(
                It.Is<SearchReservationsRequest>(request => 
                    request.Filter.SearchTerm == query.Filter.SearchTerm &&
                    request.ProviderId == query.ProviderId)));
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
            validationResult.FailedAuthorisationValidation = false;
            mockReservationService
                .Setup(service => service.SearchReservations(
                    It.Is<SearchReservationsRequest>(request => 
                        request.Filter.SearchTerm == query.Filter.SearchTerm &&
                        request.ProviderId == query.ProviderId)))
                .ReturnsAsync(serviceResponse);

            var result = await handler.Handle(query, CancellationToken.None);

            result.Reservations.Should().BeEquivalentTo(serviceResponse.Reservations);
            result.NumberOfRecordsFound.Should().Be(serviceResponse.NumberOfRecordsFound);
        }
    }
}