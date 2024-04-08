namespace SFA.DAS.Reservations.Domain.Providers.Api;

public record GetCohortAccessResponse
{
    public bool HasCohortAccess { get; set; }
}