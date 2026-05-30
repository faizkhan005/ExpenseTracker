namespace ExpenseTracker.Services.Interface
{
    public interface IBudgetService
    {
        Task<decimal> GetMonthlyBudgetAsync(int year, int month);
        Task SetMonthlyBudgetAsync(int year, int month, decimal amount);
    }
}
