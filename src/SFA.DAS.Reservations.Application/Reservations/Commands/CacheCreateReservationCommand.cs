using System;
using MediatR;

namespace SFA.DAS.Reservations.Application.Reservations.Commands
{
    public class CacheCreateReservationCommand: IRequest<CacheReservationResult>
    {
        public Guid? Id { get; set; }
        public long AccountId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public string AccountLegalEntityName { get; set; }
        public string StartDate { get; set; }
        public string StartDateDescription { get; set; }
        public string CourseId { get; set; }
        public string CourseDescription { get; set; }

        //TODO: Remove this once we work out how to handle the issue
        // with different work flow steps between employer and provider 
        public bool IgnoreStartDate { get; set; }
    }
}