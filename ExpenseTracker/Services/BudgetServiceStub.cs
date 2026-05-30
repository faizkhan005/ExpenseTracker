using ExpenseTracker.Services.Interface;

namespace ExpenseTracker.Services
{
    public class BudgetServiceStub : IBudgetService
    {
        public Task<decimal> GetMonthlyBudgetAsync(int year, int month) => Task.FromResult(3000m);
        public Task SetMonthlyBudgetAsync(int year, int month, decimal amount) => Task.CompletedTask;
    }
}
