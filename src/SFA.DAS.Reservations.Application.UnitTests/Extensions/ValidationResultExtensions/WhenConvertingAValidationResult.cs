using System;
using System.Collections.Generic;
using NUnit.Framework;
using SFA.DAS.Reservations.Application.Extensions;
using SFA.DAS.Reservations.Application.Validation;

namespace SFA.DAS.Reservations.Application.UnitTests.Extensions.ValidationResultExtensions
{
    public class WhenConvertingAValidationResult
    {
        [Test]
        public void ThenShouldFormatErrorMessage()
        {
            //Assign
            var expectedErrorMessage =
                $"The following parameters have failed validation{Environment.NewLine}Test|Test error{Environment.NewLine}Test2|Test error2";

            var sourceValidationResult = new ValidationResult
            {
                ValidationDictionary = new Dictionary<string, string> {{"Test", "Test error"}, {"Test2", "Test error2"}}
            };

            //Act
            var targetValidationResult = sourceValidationResult.ConvertToDataAnnotationsValidationResult();
            

            //Assert
            Assert.AreEqual(expectedErrorMessage, targetValidationResult.ErrorMessage);
        }
    }
}
