using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.Reservations.Domain.Interfaces;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class DeleteCachedReservationCommandHandler : IRequestHandler<DeleteCachedReservationCommand>
    {
        private readonly ICacheStorageService _cacheStorageService;

        public DeleteCachedReservationCommandHandler(ICacheStorageService cacheStorageService)
        {
            _cacheStorageService = cacheStorageService;
        }
        public async Task<Unit> Handle(DeleteCachedReservationCommand request, CancellationToken cancellationToken)
        {
            await _cacheStorageService.DeleteFromCache(request.Id.ToString());
            return Unit.Value;
        }
    }
}