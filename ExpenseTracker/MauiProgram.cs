using CommunityToolkit.Maui;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Application.Services;
using ExpenseTracker.Infrastructure.Data;
using ExpenseTracker.Infrastructure.Data.Repositories;
using ExpenseTracker.ViewModels;
using ExpenseTracker.Views;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Maui;
using Microsoft.Extensions.Logging;
using Plugin.Maui.OCR;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace ExpenseTracker
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseLiveCharts()
                .UseSkiaSharp()
                .UseOcr()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

            LiveCharts.Configure(config => config.AddSkiaSharp());

            //Infrastructure — DB context (singleton: one connection for app lifetime)
            string dbPath = Path.Combine(FileSystem.AppDataDirectory, "expense_tracker.db3");
            builder.Services.AddSingleton<AppDbContext>(sp => new AppDbContext(dbPath));

            //Infrastructure — Repositories (singleton: stateless, share DB context
            builder.Services.AddSingleton<IExpenseRepository, ExpenseRepository>();
            builder.Services.AddSingleton<ICategoryRepository, CategoryRepository>();
            builder.Services.AddSingleton<ILineItemRepository, LineItemRepository>();
            builder.Services.AddSingleton<IBudgetRepository, BudgetRepository>();
            builder.Services.AddSingleton<IRecurringExpenseRepository, RecurringExpenseRepository>();
            builder.Services.AddSingleton<ISmsRuleRepository, SmsRuleRepository>();
            builder.Services.AddSingleton<ISavedLocationRepository, SavedLocationRepository>();
            builder.Services.AddSingleton<IDeleteDBRepository, DeleteDBRepository>();
            builder.Services.AddSingleton<IFeedbackRepository, FeedbackRepository>();

            //Application — Services
            builder.Services.AddSingleton<IExpenseService, ExpenseService>();
            builder.Services.AddSingleton<IBudgetService, BudgetService>();
            builder.Services.AddSingleton<IRecurringExpenseService, RecurringExpenseService>();
            builder.Services.AddSingleton<ICategoryService, CategoryService>();
            builder.Services.AddSingleton<IIntelligenceService, IntelligenceService>();
            builder.Services.AddSingleton<IExportService, ExportService>();
            builder.Services.AddSingleton<IReceiptLineClassifier, RuleBasedReceiptClassifier>();
            builder.Services.AddSingleton<Application.Interfaces.IOcrService,GoogleMlKitOcrService>();
            // Once you have a trained model, swap to:
            // builder.Services.AddSingleton<IReceiptLineClassifier, MlNetReceiptClassifier>();

            //ViewModels
            builder.Services.AddTransient<AddExpenseViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<ExpensesListViewModel>();
            builder.Services.AddTransient<InsightsViewModel>();
            builder.Services.AddTransient<NotificationsViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

            //Views
            builder.Services.AddTransient<AddExpensePage>();
            builder.Services.AddTransient<Dashboard>();
            builder.Services.AddTransient<ExpensesListPage>();
            builder.Services.AddTransient<InsightsPage>();
            builder.Services.AddTransient<NotificationsPage>();
            builder.Services.AddTransient<SettingsPage>();

            //Routes
            Routing.RegisterRoute("AddExpensePage", typeof(AddExpensePage));
            Routing.RegisterRoute("NotificationsPage", typeof(Views.NotificationsPage));

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
