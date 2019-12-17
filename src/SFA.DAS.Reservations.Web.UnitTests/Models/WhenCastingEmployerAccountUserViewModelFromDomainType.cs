using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Domain.Employers;
using SFA.DAS.Reservations.Web.Models;

namespace SFA.DAS.Reservations.Web.UnitTests.Models
{
    public class WhenCastingEmployerAccountUserViewModelFromDomainType
    {
        [Test, AutoData]
        public void Then_Maps_Fields_From_Domain_Type(
            EmployerAccountUser source)
        {
            EmployerAccountUserViewModel result = source;

            result.Name.Should().Be(source.Name);
            result.Email.Should().Be(source.Email);
        }
    }
}