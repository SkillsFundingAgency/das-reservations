using System;
using System.Collections.Generic;
using System.Text;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Queries.GetAccountReservationStatus
{
    public class GetAccountReservationStatusQuery : IRequest<GetAccountReservationStatusResponse>
    {
        public long AccountId { get; set; }
        public string TransferSenderAccountId { get; set; }
        public string HashedEmployerAccountId { get; set; }
    }
}
