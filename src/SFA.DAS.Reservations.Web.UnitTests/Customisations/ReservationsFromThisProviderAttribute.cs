using System;
using System.Reflection;
using AutoFixture;
using AutoFixture.NUnit3;
using SFA.DAS.Reservations.Application.Reservations.Queries.SearchReservations;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Customisations
{
    /// <summary>
    /// Requires a frozen <see cref="ReservationsRouteModel"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ReservationsFromThisProviderAttribute : CustomizeAttribute
    {
        public override ICustomization GetCustomization(ParameterInfo parameter)
        {
            if (parameter == null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            if (parameter.ParameterType != typeof(SearchReservationsResult))
            {
                throw new ArgumentException(nameof(parameter));
            }

            return new ArrangeReservationsFromThisProviderCustomisation();
        }
    }

    public class ArrangeReservationsFromThisProviderCustomisation : ICustomization
    {
        public void Customize(IFixture fixture)
        {
            var routeModel = fixture.Create<ReservationsRouteModel>();
            fixture.Customize<Reservation>(composer => composer
                .With(reservation => reservation.ProviderId, routeModel.UkPrn));
        }
    }
}