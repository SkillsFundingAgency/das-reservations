using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Reservations.Web.Controllers;
using SFA.DAS.Reservations.Web.Models;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.Reservations.Web.UnitTests.Employers
{
    public class WhenCallingGetTransactorSignAgreement
    {
        [Test, MoqAutoData]
        public void Then_Sets_ViewModel(
            ReservationsRouteModel routeModel,
            EmployerReservationsController controller)
        {
            var result = controller.TransactorSignAgreement(routeModel) as ViewResult;

            result.ViewName.Should().Be("TransactorSignAgreement");
            var model = result.Model as SignAgreementViewModel;
            model.BackRouteName.Should().Be(routeModel.PreviousPage);
        }

        

        //todo: employers
    }
}