using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Reservations.Queries;
using SFA.DAS.Reservations.Application.Reservations.Queries.GetCachedReservation;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Interfaces;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Reservations.Queries
{
    public class WhenGettingAReservationFromCache
    {
        private const long ExpectedAccountId = 44321;

        private Mock<IValidator<IReservationQuery>> _validator;
        private Mock<ICacheStorageService> _cacheService;
        private GetCachedReservationQueryHandler _handler;
        private readonly Guid _expectedReservationId = Guid.NewGuid();
        private readonly string _expectedStartDate = DateTime.Now.AddDays(-20).ToString();

        [SetUp]
        public void Arrange()
        {
            _validator = new Mock<IValidator<IReservationQuery>>();
            _validator.Setup(x => x.ValidateAsync(It.Is<GetCachedReservationQuery>(c =>
                    c.Id.Equals(_expectedReservationId))))
                .ReturnsAsync(new ValidationResult());
           
            _cacheService = new Mock<ICacheStorageService>();
            _cacheService.Setup(x => x.RetrieveFromCache<GetCachedReservationResult>(_expectedReservationId.ToString()))
                .ReturnsAsync(new GetCachedReservationResult
                {
                    Id = _expectedReservationId,
                    AccountId = ExpectedAccountId,
                    StartDate = _expectedStartDate
                });

            _handler = new GetCachedReservationQueryHandler(_validator.Object, _cacheService.Object);
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
        public async Task Then_The_Reservation_Is_Returned_By_Id()
        {
            //Arrange
            var command = new GetCachedReservationQuery
            {
                Id = _expectedReservationId
            };

            //Act
            var actual = await _handler.Handle(command, new CancellationToken());

            //Assert
            Assert.AreEqual(_expectedReservationId, actual.Id);
            Assert.AreEqual(_expectedStartDate, actual.StartDate);
        }
    }
}