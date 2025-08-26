using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;
using SFA.DAS.Reservations.Application.Exceptions;
using SFA.DAS.Reservations.Application.Providers.Queries.GetLegalEntityAccount;
using SFA.DAS.Reservations.Application.Providers.Queries.GetTrustedEmployers;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetProviderCacheReservationCommand;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Commitments;
using SFA.DAS.Reservations.Domain.Employers;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetProviderCacheReservationCommand
{
    public class WhenGettingAProviderCacheReservationCommand
    {
        private Mock<IMediator> _mediator;
        private Mock<IValidator<GetProviderCacheReservationCommandQuery>> _validator;
        private GetProviderCacheReservationCommandQueryHandler _handler;
        private GetProviderCacheReservationCommandQuery _query;
        private GetTrustedEmployersResponse _getTrustedEmployersResponse;
        private GetAccountLegalEntityResult _getAccountLegalEntityResponse;
        private AccountLegalEntity _expectedEmployer;
        private AccountLegalEntity _expectedAccountLegalEntity;
        private Cohort _expectedCohort;

        [SetUp]
        public void Arrange()
        {
            _mediator = new Mock<IMediator>();
            _validator = new Mock<IValidator<GetProviderCacheReservationCommandQuery>>();
            _query = new GetProviderCacheReservationCommandQuery
            {
                UkPrn = 12,
                AccountLegalEntityPublicHashedId = "ABC123",
                CohortRef = "1234",
                CohortId = 123
            };

            _handler = new GetProviderCacheReservationCommandQueryHandler(_mediator.Object, _validator.Object, Mock.Of<ILogger<GetProviderCacheReservationCommandQueryHandler>>());

            _expectedEmployer = new AccountLegalEntity
            {
                AccountId = 1,
                AccountLegalEntityPublicHashedId = _query.AccountLegalEntityPublicHashedId,
                AccountLegalEntityName = "Test Employer",
                AccountLegalEntityId = 123,
                AccountName = "Test Account",
                AccountPublicHashedId = "DEF123"
            };

            _expectedAccountLegalEntity = new AccountLegalEntity
            {
                AccountId = 1,
                AccountLegalEntityPublicHashedId = _query.AccountLegalEntityPublicHashedId,
                AccountLegalEntityName = "Test Employer",
                AccountLegalEntityId = 123,
                LegalEntityId = 456
            };

            _expectedCohort = new Cohort()
            {
                CohortId = 123,
                AccountLegalEntityId = _expectedAccountLegalEntity.AccountLegalEntityId,
                LegalEntityName = _expectedAccountLegalEntity.AccountLegalEntityName,
                ProviderName = "Test Provider",
                IsFundedByTransfer = false,
                WithParty = CohortParty.Provider
            };

        _getTrustedEmployersResponse = new GetTrustedEmployersResponse
            {
                Employers = new [] { _expectedEmployer }
            };

            _getAccountLegalEntityResponse = new GetAccountLegalEntityResult
            {
                LegalEntity = _expectedAccountLegalEntity
            };

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(_getTrustedEmployersResponse);

            _mediator.Setup(mediator => mediator.Send(
                            It.IsAny<GetAccountLegalEntityQuery>(),
                            It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_getAccountLegalEntityResponse);

            _mediator.Setup(m => m.Send(
                    It.IsAny<GetCohortQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetCohortResponse {Cohort = _expectedCohort});

            _validator.Setup(v => v.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>()});
        }

        [Test]
        public async Task ThenAProvidedTrustedEmployersAreRetrieved()
        {
            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _mediator.Verify(mediator => mediator.Send(
                    It.Is<GetTrustedEmployersQuery>(query => query.UkPrn == _query.UkPrn),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenAMatchedTrustedEmployerDetailsAreReturnInCommand()
        {
            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.Command.Should().NotBeNull();
            result.Command.AccountLegalEntityName.Should().Be(_expectedEmployer.AccountLegalEntityName);
            result.Command.AccountLegalEntityPublicHashedId.Should().Be(_expectedEmployer.AccountLegalEntityPublicHashedId);
            result.Command.AccountLegalEntityId.Should().Be(_expectedEmployer.AccountLegalEntityId);
            result.Command.AccountId.Should().Be(_expectedEmployer.AccountId);
            result.Command.AccountName.Should().Be(_expectedEmployer.AccountName);
        }

        [Test]
        public async Task ThenIfNoTrustedEmployerIsFoundThenTheAccountLegalEntityIsRetrieved()
        {
            //Arrange
            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new AccountLegalEntity[0]
                });

            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _mediator.Verify(mediator => mediator.Send(
                    It.Is<GetAccountLegalEntityQuery>(q => 
                        q.AccountLegalEntityPublicHashedId.Equals(_query.AccountLegalEntityPublicHashedId)),
                    It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ThenIfNoTrustedEmployerIsFoundThenTheLegalEntityAccountDetailsAreUsed()
        {
            //Arrange
            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new AccountLegalEntity[0]
                });

            //Act
            var result = await _handler.Handle(_query, CancellationToken.None);

            //Assert
            result.Command.Should().NotBeNull();
            result.Command.AccountLegalEntityName.Should().Be(_expectedAccountLegalEntity.AccountLegalEntityName);
            result.Command.AccountLegalEntityPublicHashedId.Should().Be(_expectedAccountLegalEntity.AccountLegalEntityPublicHashedId);
            result.Command.AccountLegalEntityId.Should().Be(_expectedAccountLegalEntity.AccountLegalEntityId);
            result.Command.AccountId.Should().Be(_expectedAccountLegalEntity.AccountId);
        }

        [Test]
        public void ThenNeitherTheTrustedEmployerOrAccountLegalEntityIsFoundAnExceptionIsThrown()
        {
            //Arrange
            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse
                {
                    Employers = new AccountLegalEntity[0]
                });

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetAccountLegalEntityQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetAccountLegalEntityResult());

            //Act + Assert
            Assert.ThrowsAsync<AccountLegalEntityNotFoundException>(() => _handler.Handle(_query, CancellationToken.None));
        }

        [Test]
        public void ThenIfValidationFailsAnExceptionIsThrown()
        {
            //Arrange
            _validator.Setup(v => v.ValidateAsync(_query))
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>{{"Error", "Test Error"}}});

            //Act + Assert
            Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(_query, CancellationToken.None));
        }

        [Test]
        public async Task ThenIfUsingLegalEntityWillCheckItMatchesCohortDetails()
        {
            //Arrange
            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = new AccountLegalEntity[0]});

            //Act
            await _handler.Handle(_query, CancellationToken.None);

            //Assert
            _mediator.Verify(m => m.Send(It.Is<GetCohortQuery>(q => q.CohortId.Equals(_query.CohortId)), 
                It.IsAny<CancellationToken>()),Times.Once);
        }

        [Test]
        public void ThenIfLegalEntityDoesNotMatchCohortDetailsThrownProviderNotAuthorisedException()
        {
            //Arrange
            _expectedCohort.AccountLegalEntityId = 1111;

            _mediator.Setup(mediator => mediator.Send(
                    It.IsAny<GetTrustedEmployersQuery>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new GetTrustedEmployersResponse{Employers = new AccountLegalEntity[0]});

            //Act + Assert
            Assert.ThrowsAsync<ProviderNotAuthorisedException>(() => _handler.Handle(_query, CancellationToken.None));
        }
    }
}
