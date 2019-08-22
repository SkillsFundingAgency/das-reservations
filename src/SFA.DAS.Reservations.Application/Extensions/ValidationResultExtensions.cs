using System;
using System.Linq;

namespace SFA.DAS.Reservations.Application.Extensions
{
    public static class ValidationResultExtensions
    {
        public static System.ComponentModel.DataAnnotations.ValidationResult ConvertToDataAnnotationsValidationResult(
            this Validation.ValidationResult result)
        {
            var errorMessages = result.ErrorList.Aggregate((x, y) => $"{x}{Environment.NewLine}{y}");

            return new System.ComponentModel.DataAnnotations.ValidationResult(
                $"The following parameters have failed validation{Environment.NewLine}{errorMessages}",
                result.ErrorList);
        }
    }
}
