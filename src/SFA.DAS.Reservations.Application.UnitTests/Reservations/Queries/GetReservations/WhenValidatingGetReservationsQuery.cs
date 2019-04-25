using System.Threading.Tasks;
using AutoFixture.NUnit3;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetReservations;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetReservations
{
    [TestFixture]
    public class WhenValidatingGetReservationsQuery
    {
        [Test, AutoData, Ignore("in progress")]
        public async Task And_Account_Id_Less_Than_1_Then_Invalid(
            GetReservationsQueryValidator validator)
        {
            var query = new GetReservationsQuery();
            await validator.ValidateAsync(query);
        }
    }

    public class GetReservationsQueryValidator : IValidator<GetReservationsQuery>
    {
        public async Task<ValidationResult> ValidateAsync(GetReservationsQuery query)
        {
            await Task.CompletedTask;
            throw new System.NotImplementedException();
        }
    }
}