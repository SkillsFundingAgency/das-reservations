namespace SFA.DAS.Reservations.Application.Reservations.Services
{
    public interface IHashingService
    {
        long DecodeValue(string id);
        string HashValue(long id);
    }
}