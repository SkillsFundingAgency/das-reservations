namespace SFA.DAS.Reservations.Domain.Interfaces
{
    public interface ISessionStorageService<T>
    {
        void Store(T testModel);
        T Get();
        void Delete();
    }
}