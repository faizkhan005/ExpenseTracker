namespace ExpenseTracker.Domain.Enums;

public enum ExpenseSource
{
    Manual,       // User typed it in
    Sms,          // Parsed from bank SMS
    Ocr,          // Scanned from receipt photo
    Recurring,    // Auto-logged from a recurring rule
    Location      // Triggered by geofence prompt
}
