using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservation;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingADeleteViewModel
    {
        [Test, AutoData]
        public void Then_Sets_ReservationId(
            GetReservationResult getReservationResult)
        {
            var viewModel = new DeleteViewModel(getReservationResult);

            viewModel.ReservationId.Should().Be(getReservationResult.ReservationId);
        }

        [Test, AutoData]
        public void Then_Sets_AccountLegalEntityName(
            GetReservationResult getReservationResult)
        {
            var viewModel = new DeleteViewModel(getReservationResult);

            viewModel.AccountLegalEntityName.Should().Be(getReservationResult.AccountLegalEntityName);
        }

        [Test, AutoData]
        public void Then_Sets_CourseDescription(
            GetReservationResult getReservationResult)
        {
            var viewModel = new DeleteViewModel(getReservationResult);

            viewModel.CourseDescription.Should().Be(getReservationResult.Course.CourseDescription);
        }

        [Test, AutoData]
        public void And_Course_Null_Then_Sets_CourseDescription_Null(
            GetReservationResult getReservationResult)
        {
            getReservationResult.Course = null;

            var viewModel = new DeleteViewModel(getReservationResult);

            viewModel.CourseDescription.Should().Be("Unknown");
        }

        [Test, AutoData]
        public void Then_Sets_StartDateDescription(
            GetReservationResult getReservationResult)
        {
            var expectedDateDescription = new StartDateModel
            {
                StartDate = getReservationResult.StartDate, 
                EndDate = getReservationResult.ExpiryDate
            }.ToString();

            var viewModel = new DeleteViewModel(getReservationResult);

            viewModel.StartDateDescription.Should().Be(expectedDateDescription);
        }
    }
}