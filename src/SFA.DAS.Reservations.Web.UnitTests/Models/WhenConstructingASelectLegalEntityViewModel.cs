using System.Collections.Generic;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Models;
using api = SFA.DAS.EAS.Account.Api.Types;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    [TestFixture]
    public class WhenConstructingASelectLegalEntityViewModel
    {
        [Test, AutoData]
        public void Then_Sets_RouteModel(
            ReservationsRouteModel routeModel,
            List<api.LegalEntityViewModel> apiLegalEntities)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, apiLegalEntities);

            viewModel.RouteModel.Should().Be(routeModel);
        }

        [Test, AutoData]
        public void Then_Sets_LegalEntities(
            ReservationsRouteModel routeModel,
            List<api.LegalEntityViewModel> apiLegalEntities)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, apiLegalEntities);

            viewModel.LegalEntities.Should().BeEquivalentTo(apiLegalEntities, option => option.ExcludingMissingMembers());
        }
        
        [Test, AutoData]
        public void And_Api_LegalEntities_Null_Then_Sets_LegalEntities_Null(
            ReservationsRouteModel routeModel,
            List<api.LegalEntityViewModel> apiLegalEntities)
        {
            var viewModel = new SelectLegalEntityViewModel(routeModel, null);

            viewModel.LegalEntities.Should().BeNull();
        }
    }
}