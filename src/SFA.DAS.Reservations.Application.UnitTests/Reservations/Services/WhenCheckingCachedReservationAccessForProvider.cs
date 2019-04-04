using System;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Services;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Services
{
    public class WhenCheckingCachedReservationAccessForProvider
    {
        [Test, AutoData]
        public void Then_Allows_Access_If_UkPrn_Matches(
            ReservationAuthorisationService service,
            CachedReservation reservation)
        {
           //Arrange
            var providerUkPrn = reservation.UkPrn;

            //Act
            var result = service.ProviderReservationAccessAllowed(providerUkPrn, reservation);

            //Assert
            Assert.IsTrue(result);
        }

        [Test, AutoData]
        public void Then_Denies_Access_If_UkPrn_Doesnt_Matches(
            ReservationAuthorisationService service,
            CachedReservation reservation)
        {
            //Arrange
            var providerUkPrn = reservation.UkPrn + 10;

            //Act
            var result = service.ProviderReservationAccessAllowed(providerUkPrn, reservation);

            //Assert
            Assert.IsFalse(result);
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_UkPrn_Default_Value(
            ReservationAuthorisationService service,
            CachedReservation reservation)
        {
            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => service.ProviderReservationAccessAllowed(default(uint), reservation));
            exception.ParamName.Should().Be("ukPrn");
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_Reservation_Is_Null(
            ReservationAuthorisationService service,
            CachedReservation reservation)
        {
            //Arrange
            var providerUkPrn = reservation.UkPrn;

            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => service.ProviderReservationAccessAllowed(providerUkPrn, null));
            exception.ParamName.Should().Be("reservation");
        }

        [Test, AutoData]
        public void Then_Exception_Thrown_If_Reservation_UkPrn_Not_Set(
            ReservationAuthorisationService service,
            CachedReservation reservation)
        {
            //Arrange
            var providerUkPrn = reservation.UkPrn;
            reservation.UkPrn = default(uint);

            //Act + Assert
            var exception = Assert.Throws<ArgumentException>(() => service.ProviderReservationAccessAllowed(providerUkPrn, reservation));
            exception.ParamName.Should().Be("reservation");
        }
    }
}
