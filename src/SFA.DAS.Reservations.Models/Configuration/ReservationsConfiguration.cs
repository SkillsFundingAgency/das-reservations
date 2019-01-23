namespace SFA.DAS.Reservations.Models.Configuration
{
    public class ReservationsConfiguration : IReservationsConfiguration
    {
        public IdentityServerConfiguration Identity { get; set; }
    }

    public interface IReservationsConfiguration
    {
        IdentityServerConfiguration Identity { get; set; }
    }
}