using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.SearchReservations
{
    public class WhenValidatingASearchReservationsQuery
    {

        [Test, MoqAutoData]
        public async Task Then_Is_Not_Valid_If_No_UkPrn_Is_Set(
            SearchReservationsQuery query,
            [Frozen] Mock<IReservationsOuterService> reservationsOuterService,
            SearchReservationsQueryValidator validator)
        {
            query.ProviderId = 0;

            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeFalse();
            result.ErrorList.Should().Contain("ProviderId|ProviderId has not been supplied");
            reservationsOuterService.Verify(x => x.GetTrustedEmployers(It.IsAny<uint>()), Times.Never);
        }

        [Test, MoqAutoData]
        public async Task Then_Is_Valid_When_Provider_Has_Permissions(
            SearchReservationsQuery query,
            [Frozen] Mock<IReservationsOuterService> reservationsOuterService,
            SearchReservationsQueryValidator validator)
        {
            reservationsOuterService.Setup(x => x.GetTrustedEmployers(query.ProviderId))
                .ReturnsAsync(new List<Employer> { new Employer() });

            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeFalse();
        }

        [Test, MoqAutoData]
        public async Task Then_If_Provider_Has_No_Permission_Value_Is_Set_Then_Failed_Authorisation_Is_Set_To_True(
            SearchReservationsQuery query,
            [Frozen] Mock<IReservationsOuterService> reservationsOuterService,
            SearchReservationsQueryValidator validator)
        {
            reservationsOuterService.Setup(x => x.GetTrustedEmployers(query.ProviderId))
                .ReturnsAsync(new List<Employer>());

            var result = await validator.ValidateAsync(query);

            result.IsValid().Should().BeTrue();
            result.FailedAuthorisationValidation.Should().BeTrue();
        }
    }
}