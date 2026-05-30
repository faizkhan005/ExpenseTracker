using ExpenseTracker.Models;
using ExpenseTracker.Services.Interface;

namespace ExpenseTracker.Services
{
    public class ExpenseServiceStub : IExpenseService
    {
        private readonly List<Expense> _data = new()
    {
        new Expense { Id = 1, Name = "Walmart Grocery",   Amount = 84.50m,  Date = DateTime.Today,             IsIncome = false, Category = new Category { Name = "Food"      } },
        new Expense { Id = 2, Name = "Spotify Premium",   Amount = 9.99m,   Date = DateTime.Today,             IsIncome = false, Category = new Category { Name = "Subscriptions" } },
        new Expense { Id = 3, Name = "Shell Gas Station", Amount = 52.00m,  Date = DateTime.Today.AddDays(-1), IsIncome = false, Category = new Category { Name = "Transport" } },
        new Expense { Id = 4, Name = "Chick-fil-A",       Amount = 14.80m,  Date = DateTime.Today.AddDays(-1), IsIncome = false, Category = new Category { Name = "Dining"    } },
        new Expense { Id = 5, Name = "Rent",              Amount = 1200.00m,Date = DateTime.Today.AddDays(-2), IsIncome = false, Category = new Category { Name = "Housing"   } },
        new Expense { Id = 6, Name = "Salary deposit",    Amount = 3200.00m,Date = DateTime.Today.AddDays(-4), IsIncome = true,  Category = new Category { Name = "Income"    } },
    };

        public Task<List<Expense>> GetExpensesAsync(DateTime from, DateTime to)
            => Task.FromResult(_data.Where(e => e.Date >= from && e.Date <= to).ToList());

        public Task<decimal> GetIncomeAsync(int year, int month)
            => Task.FromResult(_data.Where(e => e.IsIncome && e.Date.Year == year && e.Date.Month == month).Sum(e => e.Amount));

        public Task AddExpenseAsync(Expense expense) { _data.Add(expense); return Task.CompletedTask; }
        public Task UpdateExpenseAsync(Expense expense) { return Task.CompletedTask; }
        public Task DeleteExpenseAsync(int id) { _data.RemoveAll(e => e.Id == id); return Task.CompletedTask; }
    }
}
