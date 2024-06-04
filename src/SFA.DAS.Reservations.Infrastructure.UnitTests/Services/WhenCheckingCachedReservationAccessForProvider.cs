using System;
using System.Collections.Generic;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Services;

namespace SFA.DAS.Reservations.Infrastructure.UnitTests.Services
{
    public class WhenCheckingCachedReservationAccessForProvider
    {
        private ReservationAuthorisationService _service;
        private CachedReservation _reservation;
        private Employer _employer;
        private Mock<IReservationsOuterService> _reservationsOuterService;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization { ConfigureMembers = true });

            _reservation = fixture.Create<CachedReservation>();
            _employer = fixture.Create<Employer>();
            _reservationsOuterService = new Mock<IReservationsOuterService>();
            _service = new ReservationAuthorisationService(_reservationsOuterService.Object);

            _reservation.AccountLegalEntityId = _employer.AccountLegalEntityId;

            _reservationsOuterService.Setup(s => s.GetTrustedEmployers(It.IsAny<uint>()))
                .ReturnsAsync(new List<Employer> { _employer });
        }

        [Test, AutoData]
        public void Then_Allows_Access_If_UkPrn_Matches()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn.Value;

            //Act
            var result = _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation);

            //Assert
            Assert.IsTrue(result);
        }

        [Test, AutoData]
        public void Then_Denies_Access_If_UkPrn_Doesnt_Matches()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn.Value + 10;

            //Act
            var result = _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_UkPrn_Default_Value()
        {
            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ProviderReservationAccessAllowed(default(uint), _reservation));
            exception.ParamName.Should().Be("ukPrn");
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_Reservation_Is_Null()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn.Value;

            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ProviderReservationAccessAllowed(providerUkPrn, (CachedReservation)null));
            exception.ParamName.Should().Be("reservation");
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_Reservation_UkPrn_Not_Set()
        {
            //Arrange
            var providerUkPrn = _reservation.UkPrn.Value;
            _reservation.UkPrn = default(uint);

            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => _service.ProviderReservationAccessAllowed(providerUkPrn, _reservation));
            exception.ParamName.Should().Be("reservation");
        }
    }
}
