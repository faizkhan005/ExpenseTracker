namespace ExpenseTracker.Application.Interfaces
{
    public interface IDeleteDBRepository
    {
        Task ClearAllDataAsync();
    }
}
