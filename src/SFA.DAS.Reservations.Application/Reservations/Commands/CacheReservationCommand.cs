using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheReservationCommand: BaseCreateReservationCommand, IRequest<CacheReservationResult>
    {

    }
}