using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Infrastructure.Configuration;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenBuildingManageViewModel
    {
        [TestCase(0,false)]
        [TestCase(9,false)]
        [TestCase(10,true)]
        [TestCase(11,true)]
        [TestCase(100,true)]
        public void Then_If_The_Hide_Search_Flag_Is_Set_Based_On_Number_Of_Reservations(int numberOfReservations, bool expectedBool)
        {
            //Act
            var actual = new ManageViewModel
            {
                TotalReservationCount = numberOfReservations
            };

            //Assert
            Assert.AreEqual(expectedBool, actual.ShowSearch);
        }

        [TestCase(0, false)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsPerSearchPage - 1, false)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsPerSearchPage, false)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsPerSearchPage + 1, true)]
        public void Then_The_Hide_Footer_Flag_Is_Set_Based_On_Number_Of_Filtered_Reservations(
            int numberOfFilteredReservations, bool expectedBool)
        {
            //Arrange
            var reservationViewModel = new ReservationViewModel(new Reservation(), "", null);

            var reservations = new List<ReservationViewModel>();

            for (var i = 0; i < numberOfFilteredReservations; i++)
            {
                reservations.Add(reservationViewModel);
            }

            //Act
            var actual = new ManageViewModel {Reservations = reservations};

            //Assert
            Assert.AreEqual(expectedBool, actual.ShowFilteredSearch);
        }
    }
}