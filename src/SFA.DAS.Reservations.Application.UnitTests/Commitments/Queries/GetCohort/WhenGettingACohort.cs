using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using Moq;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Commitments.Queries.GetCohort;
using SFA.DAS.Reservations.Application.Commitments.Services;
using SFA.DAS.Reservations.Application.Validation;
using SFA.DAS.Reservations.Domain.Commitments;
using SFA.DAS.Testing.AutoFixture;
using ValidationResult = SFA.DAS.Reservations.Application.Validation.ValidationResult;

namespace SFA.DAS.Reservations.Application.UnitTests.Commitments.Queries.GetCohort
{
    public class WhenGettingACohort
    {
        [Test, MoqAutoData]
        public async Task ThenWillGetCohortFromService(
            GetCohortQuery query,
            Cohort expectedCohort,
            [Frozen] Mock<ICommitmentService> mockCommitmentService,
            [Frozen] Mock<IValidator<GetCohortQuery>> mockValidator,
            GetCohortQueryHandler handler)
        {
            //Arrange
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetCohortQuery>()))
                         .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>()});

            mockCommitmentService.Setup(s => s.GetCohort(It.IsAny<long>()))
                .ReturnsAsync(expectedCohort);

            //Act
            var result = await handler.Handle(query, CancellationToken.None);

            //Assert
            Assert.AreEqual(expectedCohort, result.Cohort);
        }

        [Test, MoqAutoData]
        public async Task ThenWillUseCorrectCohortId(
            Cohort expectedCohort,
            [Frozen] GetCohortQuery query,
            [Frozen] Mock<ICommitmentService> mockCommitmentService,
            [Frozen] Mock<IValidator<GetCohortQuery>> mockValidator,
            GetCohortQueryHandler handler)
        {
            //Arrange
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetCohortQuery>()))
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>()});

            mockCommitmentService.Setup(s => s.GetCohort(It.IsAny<long>()))
                .ReturnsAsync(expectedCohort);

            //Act
            await handler.Handle(query, CancellationToken.None);

            //Assert
            mockCommitmentService.Verify(s => s.GetCohort(query.CohortId), Times.Once);
        }

        [Test, MoqAutoData]
        public async Task ThenWillThrowExceptionIfOneOccurs(
            GetCohortQuery query,
            Exception exception,
            [Frozen] Mock<ICommitmentService> mockCommitmentService,
            [Frozen] Mock<IValidator<GetCohortQuery>> mockValidator,
            GetCohortQueryHandler handler)
        {
            //Arrange
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetCohortQuery>()))
                .ReturnsAsync(new ValidationResult{ValidationDictionary = new Dictionary<string, string>()});

            mockCommitmentService.Setup(s => s.GetCohort(It.IsAny<long>()))
                .ThrowsAsync(exception);

            //Act
            var actualException = Assert.ThrowsAsync<Exception>(() => handler.Handle(query, CancellationToken.None));

            //Assert
            Assert.AreEqual(exception, actualException);
        }

        [Test, MoqAutoData]
        public async Task ThenWillThrowExceptionIfValidationFails(
            GetCohortQuery query,
            [Frozen] Mock<ICommitmentService> mockCommitmentService,
            [Frozen] Mock<IValidator<GetCohortQuery>> mockValidator,
            GetCohortQueryHandler handler)
        {
            //Arrange
            mockValidator.Setup(v => v.ValidateAsync(It.IsAny<GetCohortQuery>())).ReturnsAsync(new ValidationResult
                {ValidationDictionary = new Dictionary<string, string> {{"Error", "Test Error"}}});

            //Act + Assert
            Assert.ThrowsAsync<ValidationException>(() => handler.Handle(query, CancellationToken.None));            
        }
    }
}
