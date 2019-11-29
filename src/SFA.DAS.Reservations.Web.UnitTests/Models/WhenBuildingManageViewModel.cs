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
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsRequiredForSearch-1, false)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsRequiredForSearch, true)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsRequiredForSearch+1, true)]
        [TestCase(ReservationsWebConfigurationConstants.NumberOfReservationsRequiredForSearch+100, true)]
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
            //Act
            var actual = new ManageViewModel {TotalReservationCount = numberOfFilteredReservations};

            //Assert
            Assert.AreEqual(expectedBool, actual.ShowFilteredSearch);
        }
    }
}