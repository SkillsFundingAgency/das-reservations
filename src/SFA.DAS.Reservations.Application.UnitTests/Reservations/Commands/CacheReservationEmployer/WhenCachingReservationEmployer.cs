﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Reservations.Commands.CacheReservationEmployer;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using SFA.DAS.Reservations.Domain.Rules;
using SFA.DAS.Reservations.Domain.Rules.Api;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Commands.CacheReservationEmployer
{
    public class WhenCachingReservationEmployer
    {
        private Mock<IValidator<CacheReservationEmployerCommand>> _mockValidator;
        private Mock<ICacheStorageService> _mockCacheStorageService;
        private Mock<IFundingRulesService> _mockFundingRulesService;
        private CacheReservationEmployerCommandHandler _commandHandler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture()
                .Customize(new AutoMoqCustomization{ConfigureMembers = true});

            _mockValidator = fixture.Freeze<Mock<IValidator<CacheReservationEmployerCommand>>>();
           
            _mockValidator
                .Setup(validator => validator.ValidateAsync(It.IsAny<CacheReservationEmployerCommand>()))
                .ReturnsAsync(new ValidationResult());

            _mockCacheStorageService = fixture.Freeze<Mock<ICacheStorageService>>();
            _mockFundingRulesService = fixture.Freeze<Mock<IFundingRulesService>>();

            _commandHandler = fixture.Create<CacheReservationEmployerCommandHandler>();
        }

        [Test, AutoData]
        public async Task Then_It_Validates_The_Command(CacheReservationEmployerCommand command)
        {
            var response = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
            };

            _mockFundingRulesService.Setup(m => m.GetAccountFundingRules(It.IsAny<long>()))
                .ReturnsAsync(response);
             
            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockValidator.Verify(validator => validator.ValidateAsync(command), Times.Once);
        }

        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Throws_ArgumentException(
            CacheReservationEmployerCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);

            //Act
            Func<Task> act = async () => { await _commandHandler.Handle(command, CancellationToken.None); };

            //Assert
            act.Should().ThrowExactly<ValidationException>()
                .Which.ValidationResult.MemberNames.First(c=>c.StartsWith(propertyName)).Should().NotBeNullOrEmpty();
        }

        [Test, AutoData]
        public void And_If_There_Are_Reservation_Rules_Then_Throws_ReservationLimitReached_Exception(
            CacheReservationEmployerCommand command)
        {
            //Arrange
            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult{FailedRuleValidation = true,ValidationDictionary = new Dictionary<string, string>()});

            //Act + Assert
            Assert.ThrowsAsync<ReservationLimitReachedException>( () => _commandHandler.Handle(command, CancellationToken.None));

        }

        [Test, AutoData]
        public void And_If_There_Are_Global_Rules_Then_Throws_GlobalReservationRule_Exception(
            CacheReservationEmployerCommand command)
        {
            //Arrange
            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult { FailedGlobalRuleValidation = true, ValidationDictionary = new Dictionary<string, string>() });

            //Act + Assert
            Assert.ThrowsAsync<GlobalReservationRuleException>(() => _commandHandler.Handle(command, CancellationToken.None));

        }

        [Test, AutoData]
        public void And_Provider_Is_Not_Trusted_With_Legal_Entity_Then_Throws_Provider_Not_Authorised_Exception(
            CacheReservationEmployerCommand command)
        {
            //Arrange
            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(new ValidationResult{FailedAuthorisationValidation = true,ValidationDictionary = new Dictionary<string, string>()});

            //Act + Assert
            var exception = Assert.ThrowsAsync<ProviderNotAuthorisedException>( () => _commandHandler.Handle(command, CancellationToken.None));
            
            Assert.AreEqual(command.AccountId, exception.AccountId);
            Assert.AreEqual(command.UkPrn, exception.UkPrn);
        }
        
        [Test, AutoData]
        public void And_The_Command_Is_Not_Valid_Then_Does_Not_Cache_Reservation(
            CacheReservationEmployerCommand command,
            ValidationResult validationResult,
            string propertyName)
        {
            //Assign
            validationResult.AddError(propertyName);

            _mockValidator
                .Setup(validator => validator.ValidateAsync(command))
                .ReturnsAsync(validationResult);
            
            //Act
             Assert.ThrowsAsync<ValidationException>(() => _commandHandler.Handle(command, CancellationToken.None));

            //Assert
            _mockCacheStorageService.Verify(s => s.SaveToCache(It.IsAny<string>(), It.IsAny<CachedReservation>(), It.IsAny<int>()), Times.Never);
        }

        [Test, AutoData]
        public async Task Then_Calls_Cache_Service_To_Save_Reservation(CacheReservationEmployerCommand command)
        {
            GetAccountFundingRulesApiResponse response = new GetAccountFundingRulesApiResponse()
            {
                GlobalRules = new List<GlobalRule>()
            };

            command.UkPrn = null;
            command.EmployerHasSingleLegalEntity = true;

            _mockFundingRulesService.Setup(c => c.GetAccountFundingRules(It.IsAny<long>()))
                .ReturnsAsync(response);

            //Act
            await _commandHandler.Handle(command, CancellationToken.None);

            //Assert
            _mockCacheStorageService.Verify(service => service.SaveToCache(
                It.IsAny<string>(), 
                It.Is<CachedReservation>(c => c.Id.Equals(command.Id) &&
                    c.AccountId.Equals(command.AccountId) &&
                    c.AccountLegalEntityId.Equals(command.AccountLegalEntityId) &&
                    c.AccountLegalEntityName.Equals(command.AccountLegalEntityName) &&
                    c.AccountLegalEntityPublicHashedId.Equals(command.AccountLegalEntityPublicHashedId) &&
                    c.AccountName.Equals(command.AccountName) &&
                    c.CohortRef.Equals(command.CohortRef) &&
                    c.UkPrn.Equals(command.UkPrn) &&
                    c.IsEmptyCohortFromSelect.Equals(command.IsEmptyCohortFromSelect) &&
                    c.EmployerHasSingleLegalEntity.Equals(command.EmployerHasSingleLegalEntity)), 
                1));
        }
    }
}
