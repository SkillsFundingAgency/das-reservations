using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Extensions;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingAnAvailableReservationViewModel
    {
        [Test, AutoData]
        public void Then_Sets_ReservationId(
            Reservation reservation)
        {
            var model = new AvailableReservationViewModel(reservation);

            model.ReservationId.Should().Be(reservation.Id);
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            Reservation reservation)
        {
            var model = new AvailableReservationViewModel(reservation);

            model.CourseDescription.Should().Be(reservation.Course.CourseDescription);
        }

        [Test, AutoData]
        public void Then_Sets_TrainingDateDescription(
            Reservation reservation)
        {
            var expectedTrainingDateDescription = new TrainingDateModel
            {
                StartDate = reservation.StartDate, 
                EndDate = reservation.ExpiryDate
            }.GetGDSDateString();

            var model = new AvailableReservationViewModel(reservation);

            model.TrainingDateDescription.Should().Be(expectedTrainingDateDescription);
        }

        [Test, AutoData]
        public void Then_Sets_CreatedDateDescription(
            Reservation reservation)
        {
            var expectedCreatedDateDescription = reservation.CreatedDate.GetGDSLongDateString();

            var model = new AvailableReservationViewModel(reservation);

            model.CreatedDateDescription.Should().Be(expectedCreatedDateDescription);
        }
    }
}