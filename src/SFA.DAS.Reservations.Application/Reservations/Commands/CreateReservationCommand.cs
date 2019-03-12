using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CreateReservationCommand : BaseCreateReservationCommand, IRequest<CreateReservationResult>
    {

    }
}