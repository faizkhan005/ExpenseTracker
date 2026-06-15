# Expense Tracker — .NET MAUI

A cross-platform personal finance app built with .NET MAUI, following Clean Architecture principles. Tracks expenses and income, visualizes spending with interactive charts, scans receipts using on-device OCR, automates recurring expenses, and surfaces budget insights — all running fully offline on Android and iOS.

## Features

**Core tracking**
- Manual expense and income entry with custom categories
- Monthly budget targets with real-time progress tracking
- SQLite local storage — fully offline, no account required

**Visualization**
- Dashboard with spending overview, savings, and category breakdown
- Weekly bar chart and category donut chart (LiveCharts2)
- 6-month spending trend analysis

**Receipt scanning**
- On-device OCR using Google ML Kit (no cloud calls, no cost)
- Automatic line-item extraction (product name, quantity, price)
- Lightweight custom classifier to parse receipt structure (merchant, subtotal, tax, total)

**Automation**
- Recurring expenses (rent, subscriptions) auto-logged on schedule
- SMS transaction parsing for supported US banks (Android only)
- Location-based purchase prompts via geofencing

**Insights**
- Budget pacing and category-level spend tracking
- Next-month spending prediction (weighted moving average)
- Grocery quantity recommendations based on purchase history
- Personalized savings tips generated from spending patterns

**Custom categories**
- Full icon picker with 70+ icons across 9 groups
- User-created categories stored and rendered dynamically — no code changes needed for new categories

## Tech Stack

| Layer | Technology |
|---|---|
| UI Framework | .NET MAUI (.NET 8) |
| Architecture | Clean Architecture (Domain / Application / Infrastructure / Presentation) |
| MVVM | CommunityToolkit.Mvvm |
| Local Database | SQLite via sqlite-net-pcl |
| Charts | LiveCharts2 (SkiaSharp) |
| OCR | Google ML Kit via Plugin.Maui.OCR |
| Receipt parsing | Custom rule-based classifier + optional ML.NET model |
| UI Toolkit | CommunityToolkit.Maui |
| Icons | Material Icons (FontImageSource) |

## Project Structure

```
ExpenseTracker.sln
├── ExpenseTracker.Domain          — entities, enums (no dependencies)
├── ExpenseTracker.Application     — interfaces, services, business logic
├── ExpenseTracker.Infrastructure  — SQLite repositories, platform integrations
└── ExpenseTracker.MAUI            — Views, ViewModels, DI composition root
```

Dependencies flow inward: `MAUI → Infrastructure → Application → Domain`.

## Getting Started

### Prerequisites
- Visual Studio 2022 (17.8+) with the .NET MAUI workload
- .NET 8 SDK
- Android SDK (for Android target) or Xcode (for iOS target)

### Setup

```bash
git clone https://github.com/yourusername/expense-tracker.git
cd expense-tracker
dotnet restore
```

Open `ExpenseTracker.sln` in Visual Studio, select your target platform, and run.

### Required NuGet packages

```
CommunityToolkit.Mvvm
CommunityToolkit.Maui
LiveChartsCore.SkiaSharpView.Maui
sqlite-net-pcl
SQLitePCLRaw.bundle_green
Plugin.Maui.OCR
Microsoft.ML
```

### First run

The SQLite database is created automatically on first launch with seeded default categories (Food, Transport, Housing, Dining, Health, Subscriptions, Shopping, Income, Other) and sample SMS parsing rules for major US banks.

## Roadmap / Known Limitations

- **AI-generated savings tips**: not yet implemented. Currently in progress — will replace what was originally planned as a Claude API call with a fully offline template-based generation system (see `Wiki/Architecture.md` for design rationale).
- **SMS parsing and location prompts**: Android-only due to iOS platform restrictions on SMS access.
- **Receipt OCR accuracy**: currently uses a rule-based line classifier; a trainable ML.NET model is scaffolded but requires real-world receipt data to improve accuracy further.
- **iOS testing**: primary development and testing has been on Android; iOS-specific permission flows may need adjustment.

## Documentation

See the [Wiki](./Wiki) folder for:
- Detailed architecture and tech stack rationale
- Database schema reference
- ML/OCR pipeline design
- Interview preparation notes covering key design decisions

## License

MIT
