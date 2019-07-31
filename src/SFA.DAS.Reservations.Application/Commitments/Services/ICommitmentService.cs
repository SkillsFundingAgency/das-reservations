using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Commitments;

namespace SFA.DAS.Reservations.Application.Commitments.Services
{
    public interface ICommitmentService
    {
        Task<Cohort> GetCohort(long cohortId);
    }
}
