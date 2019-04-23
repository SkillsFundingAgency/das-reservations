using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingASelectLegalEntityViewModel
    {
        [Test, AutoData]
        public void Then_Sets_RouteModel(
            ReservationsRouteModel routeModel,
            List<AccountLegalEntity> accountLegalEntities)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, accountLegalEntities);

            viewModel.RouteModel.Should().Be(routeModel);
        }

        [Test, AutoData]
        public void Then_Sets_LegalEntities(
            ReservationsRouteModel routeModel,
            List<AccountLegalEntity> accountLegalEntities)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, accountLegalEntities);

            viewModel.LegalEntities.Should().BeEquivalentTo(accountLegalEntities, option => option.ExcludingMissingMembers());
        }
        
        [Test, AutoData]
        public void And_Api_LegalEntities_Null_Then_Sets_LegalEntities_Null(
            ReservationsRouteModel routeModel)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, null);

            viewModel.LegalEntities.Should().BeNull();
        }
    }
}