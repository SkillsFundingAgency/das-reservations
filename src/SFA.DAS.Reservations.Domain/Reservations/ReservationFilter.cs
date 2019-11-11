namespace SFA.DAS.Reservations.Domain.Reservations
{
    public class ReservationFilter
    {
        public string SearchTerm { get; set; }
        public int PageSize { get; set; }
        public int PageNumber { get; set; }
    }
}