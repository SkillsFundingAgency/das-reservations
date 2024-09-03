using System;

namespace SFA.DAS.Reservations.Application.Exceptions
{
    public class MustCreateViaAutoReservationRouteException : Exception
    {
        public MustCreateViaAutoReservationRouteException() : base("For Auto Reservations we do not create a reservation via cache")
        {
            
        }
    }
}


