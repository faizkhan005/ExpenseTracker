namespace ExpenseTracker.Application.Services;

public class ModelDeployment
{
    private const string ModelFileName = "receipt_classifier.zip";

    /// <summary>
    /// Copies the bundled trained model from app package (Resources/Raw)
    /// to writable AppDataDirectory on first launch. Call this in
    /// App.xaml.cs OnStart() before any OCR happens.
    /// </summary>
    public static async Task EnsureModelDeployedAsync()
    {
        var targetPath = Path.Combine(FileSystem.AppDataDirectory, ModelFileName);

        if (File.Exists(targetPath))
            return; // Already deployed

        try
        {
            using var sourceStream = await FileSystem.OpenAppPackageFileAsync(ModelFileName);
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);
        }
        catch (FileNotFoundException)
        {
            // Model not bundled yet — app will fall back to RuleBasedReceiptClassifier
            System.Diagnostics.Debug.WriteLine(
                "No trained model found in package. Using rule-based classifier.");
        }
    }
}
