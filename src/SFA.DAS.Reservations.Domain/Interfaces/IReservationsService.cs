using System.Threading.Tasks;
using SFA.DAS.Reservations.Domain.Reservations;

namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface IReservationsService
    {
        Task<GetTransferValidityResponse> GetTransferValidity(long senderId, long receiverId, int? pledgeApplicationId=null);
    }
}
