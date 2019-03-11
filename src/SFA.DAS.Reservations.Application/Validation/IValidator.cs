using System.Threading.Tasks;

namespace SFA.DAS.Reservations.Application.Validation
{
    public interface IValidator<in T>
    {
        Task<ValidationResult> ValidateAsync(T query);
    }
}