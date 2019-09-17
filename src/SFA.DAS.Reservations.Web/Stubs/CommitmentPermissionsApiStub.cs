using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Authorization.CommitmentPermissions.Client;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.Reservations.Web.Stubs
{
    public class CommitmentPermissionsApiStub : ICommitmentPermissionsApiClient
    {
        public Task<bool> CanAccessCohort(CohortAccessRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }
    }
}
