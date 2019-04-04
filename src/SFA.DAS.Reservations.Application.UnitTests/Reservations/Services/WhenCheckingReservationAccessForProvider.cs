using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Employers.Services;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Reservations.Api;
using StructureMap.Pipeline;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    public class WhenCheckingReservationAccessForProvider
    {
        private ReservationAuthorisationService _service;
        private GetReservationResponse _reservation;
        private Employer _employer;
        private Mock<IProviderPermissionsService> _providerPermissionService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _reservation = fixture.Create<GetReservationResponse>();
            _employer = fixture.Create<Employer>();
            _providerPermissionService = new Mock<IProviderPermissionsService>();
            _service = new ReservationAuthorisationService(_providerPermissionService.Object);

            _reservation.AccountLegalEntityId = _employer.AccountLegalEntityId;

            _providerPermissionService.Setup(s => s.GetTrustedEmployers(It.IsAny<uint>()))
                .ReturnsAsync(new List<Employer> {_employer});
        }

        [Test]
        public async Task Then_Provider_Has_Access_If_Allowed()
        {
           //Arrange
            var providerUkPrn = _reservation.UkPrn;

            //Act
            var result = await _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation);

            //Assert
            Assert.IsTrue(result);
        }

        [Test]
        public async Task Then_Denies_Access_If_UkPrn_Doesnt_Matches()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn + 10;

            //Act
            var result = await _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation);

            //Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Then_Exception_Thrown_If_UkPrn_Default_Value()
        {
            //Act + Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.ProviderReservationAccessAllowed(default(uint), _reservation));
            exception.ParamName.Should().Be("ukPrn");
        }

        [Test]
        public void Then_Exception_Thrown_If_Reservation_Is_Null()
        {
            //Act + Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.ProviderReservationAccessAllowed(10, (GetReservationResponse) null));
            exception.ParamName.Should().Be("reservation");
        }

        [Test]
        public void Then_Exception_Thrown_If_Reservation_UkPrn_Not_Set()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn;
            _reservation.UkPrn = default(uint);

            //Act + Assert
            var exception = Assert.ThrowsAsync<ArgumentException>(async () => await _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation));
            exception.ParamName.Should().Be("reservation");
        }

        [Test]
        public void Then_Exception_Thrown_If_Provider_Does_Not_Have_Access_To_Employer()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn;
            _reservation.AccountLegalEntityId = _employer.AccountLegalEntityId + 1;

            //Act + Assert
            Assert.ThrowsAsync<UnauthorizedAccessException>(async() => await _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation));
        }
    }
}
