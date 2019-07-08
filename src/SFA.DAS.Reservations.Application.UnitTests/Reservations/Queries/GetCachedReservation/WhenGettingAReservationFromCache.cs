using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoMoq;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using SFA.DAS.Reservations.Domain.Reservations;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries.GetCachedReservation
{
    public class WhenGettingAReservationFromCache
    {
        private CachedReservation _cachedReservation;
        private Mock<IValidator<IReservationQuery>> _validator;
        private Mock<ICachedReservationRespository> _cacheReservationRepository;
        private GetCachedReservationQueryHandler _handler;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture().Customize(new AutoMoqCustomization());
            _cachedReservation = fixture.Create<CachedReservation>();

            _validator = fixture.Freeze<Mock<IValidator<IReservationQuery>>>();
            _validator
                .Setup(x => x.ValidateAsync(It.Is<GetCachedReservationQuery>(c =>
                    c.Id.Equals(_cachedReservation.Id))))
                .ReturnsAsync(new ValidationResult());
           
            _cacheReservationRepository = fixture.Freeze<Mock<ICachedReservationRespository>>();
            
            _cacheReservationRepository
                .Setup(r => r.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()))
                .ReturnsAsync(_cachedReservation);

            _handler = fixture.Create<GetCachedReservationQueryHandler>();
        }

        [Test]
        public void Then_The_Command_Is_Validated_An_Exception_Thrown_If_Not_Valid()
        {
            //Arrange
            _validator
                .Setup(x => x.ValidateAsync(It.IsAny<GetCachedReservationQuery>()))
                .ThrowsAsync(new ValidationException(
                    new System.ComponentModel.DataAnnotations.ValidationResult("Error", new List<string>()), null,
                    null));

            //Act Assert
            Assert.ThrowsAsync<ValidationException>(async () =>
                await _handler.Handle(new GetCachedReservationQuery(), new CancellationToken()));
        }

        [Test]
        public async Task Then_The_Provider_Reservation_Is_Returned_By_Id()
        {
            //Arrange
            var command = new GetCachedReservationQuery
            {
                Id = _cachedReservation.Id,
                UkPrn = 12
            };

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.AreEqual(_cachedReservation.Id, actual.Id);
            Assert.AreEqual(_cachedReservation.TrainingDate, actual.TrainingDate);
            Assert.AreEqual(_cachedReservation.CohortRef, actual.CohortRef);

            _cacheReservationRepository.Verify(r => r.GetProviderReservation(command.Id, command.UkPrn), Times.Once);
            _cacheReservationRepository.Verify(r => r.GetEmployerReservation(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task Then_The_Employer_Reservation_Is_Returned_By_Id()
        {
            //Arrange
            _cacheReservationRepository
                .Setup(r => r.GetEmployerReservation(It.IsAny<Guid>()))
                .ReturnsAsync(_cachedReservation);

            var command = new GetCachedReservationQuery
            {
                Id = _cachedReservation.Id
            };

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.AreEqual(_cachedReservation.Id, actual.Id);
            Assert.AreEqual(_cachedReservation.TrainingDate, actual.TrainingDate);

            _cacheReservationRepository.Verify(r => r.GetEmployerReservation(command.Id), Times.Once);
            _cacheReservationRepository.Verify(r => r.GetProviderReservation(It.IsAny<Guid>(), It.IsAny<uint>()), Times.Never);
        }
    }
}