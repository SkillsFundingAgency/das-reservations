using System;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetCachedReservation
{
    [TestFixture]
    public class WhenValidatingACachedReservation
    {
        [Test, AutoData]
        public async Task And_AccountId_Less_Than_One_Then_Invalid(
            CachedReservationValidator validator)
        {
            var cachedReservation = new CachedReservation
            {
                Id = Guid.NewGuid(),
                AccountId = 0,
                TrainingDate = new TrainingDateModel{StartDate = DateTime.Now},
                AccountLegalEntityName = "Test Name"
            };

            var result = await validator.ValidateAsync(cachedReservation);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CachedReservation.AccountId))
                .WhichValue.Should().Be($"{nameof(CachedReservation.AccountId)} has not been supplied");
        }

        [Test, AutoData]
        public async Task And_All_Fields_Invalid_Then_Returns_All_Errors(
            CachedReservationValidator validator)
        {
            var cachedReservation = new CachedReservation
            {
                AccountId = 0,
                TrainingDate = null,
                AccountLegalEntityName = ""
            };

            var result = await validator.ValidateAsync(cachedReservation);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(3);
            result.ValidationDictionary
                .Should().ContainKey(nameof(CachedReservation.AccountId))
                .And.ContainKey(nameof(CachedReservation.TrainingDate))
                .And.ContainKey(nameof(CachedReservation.AccountLegalEntityName));
        }

        [Test, AutoData]
        public async Task And_Start_Date_Not_Set_Then_Returns_Error(
            CachedReservationValidator validator)
        {
            var cachedReservation = new CachedReservation
            {
                AccountId = 1,
                TrainingDate = new TrainingDateModel(),
                AccountLegalEntityName = "Test Name"
            };

            var result = await validator.ValidateAsync(cachedReservation);

            result.IsValid().Should().BeFalse();
            result.ValidationDictionary.Count.Should().Be(1);
            result.ValidationDictionary.ContainsKey(nameof(CachedReservation.TrainingDate));
        }

        [Test, AutoData]
        public async Task And_All_Fields_Valid_Then_Valid(
            CachedReservationValidator validator)
        {
            var cachedReservation = new CachedReservation
            {
                AccountId = 1,
                TrainingDate = new TrainingDateModel(){StartDate = DateTime.Now},
                AccountLegalEntityName = "Test Name"
            };

            var result = await validator.ValidateAsync(cachedReservation);

            result.IsValid().Should().BeTrue();
            result.ValidationDictionary.Count.Should().Be(0);
        }
    }
}
