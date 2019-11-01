namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class SearchReservationsRequest
    {
        public uint ProviderId { get; set; }
        public ReservationFilter Filter { get; set; }
    }
}