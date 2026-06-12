using ExpenseTracker.Application.DTO;
using ExpenseTracker.Application.Interfaces;
using Plugin.Maui.OCR;

namespace ExpenseTracker.Application.Services;

public class GoogleMlKitOcrService : Interfaces.IOcrService
{
    private readonly IReceiptLineClassifier _classifier;

    public GoogleMlKitOcrService(IReceiptLineClassifier classifier)
       => _classifier = classifier;

    public async Task<DTO.OcrResult> ScanReceiptAsync(Stream imageStream)
    {
        try
        {
            // Convert stream to byte array for Plugin.Maui.OCR
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            // ── Stage 1: Extract raw text via ML Kit ──────────────────────────
            var ocrResult = await OcrPlugin.Default.RecognizeTextAsync(imageBytes);

            if (string.IsNullOrWhiteSpace(ocrResult.AllText))
            {
                return new DTO.OcrResult
                {
                    IsSuccessful = false,
                    ErrorMessage = "No text detected in image."
                };
            }

            // Split into lines, removing empty ones
            var lines = ocrResult.AllText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Where(l => l.Length > 0)
                .ToList();

            // ── Stage 2: Classify each line using our lightweight model ──────
            return _classifier.ParseReceiptLines(lines);
        }
        catch (Exception ex)
        {
            return new DTO.OcrResult
            {
                IsSuccessful = false,
                ErrorMessage = $"OCR failed: {ex.Message}"
            };
        }
    }
}
