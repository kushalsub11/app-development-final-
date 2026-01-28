using JournalApp.Models;
using JournalApp.Data;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace JournalApp.Services
{
    /// <summary>
    /// Service for exporting journal entries to HTML/PDF
    /// Simple, student-friendly approach using HTML export
    /// </summary>
    /// <summary>
    /// Service for exporting journal entries.
    /// [VIVA INFO]: Demonstrates 'File I/O' and 'Information Presentation'.
    /// This service transforms raw database records into a professional HTML report.
    /// </summary>
        /// <summary>
        /// NOTE: Uses iText7 for PDF export.
        /// Install via NuGet: itext7
        /// </summary>
        public class ExportService
    {
        private readonly JournalService _journalService;

        public ExportService(JournalService journalService)
        {
            _journalService = journalService;
        }

        /// <summary>
        /// Generates an HTML report for a range of entries.
        /// [VIVA INFO]: HTML is used because it's cross-platform and can be 
        /// converted to PDF by any modern browser or specialized library.
        /// </summary>
        public async Task<string> ExportToPdfAsync(DateTime startDate, DateTime endDate, string filePath, int userId)
        {
            try
            {
                // [LOGIC]: Delegate data fetching to the JournalService (Separation of Concerns).
                var entries = await _journalService.FilterByDateRangeAsync(startDate, endDate, userId);

                if (!entries.Any()) return "No entries found for the selected date range.";

                // [FILE SYSTEM]: Normalize output path and ensure directory exists.
                if (Directory.Exists(filePath) || string.IsNullOrWhiteSpace(Path.GetFileName(filePath)))
                {
                    filePath = Path.Combine(filePath, "JournalExport.pdf");
                }

                filePath = Path.ChangeExtension(filePath, ".pdf");
                var directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // [PDF EXPORT]: Use iText7 to create the PDF
                Console.WriteLine($"[ExportService] Exporting PDF to: {filePath}");
                using (var writer = new PdfWriter(filePath))
                using (var pdf = new PdfDocument(writer))
                using (var document = new Document(pdf))
                {
                    document.Add(new Paragraph("Personal Reflection Report")
                        .SetBold()
                        .SetFontSize(18));

                    foreach (var entry in entries.OrderBy(e => e.EntryDate))
                    {
                        document.Add(new Paragraph(entry.EntryDate.ToString("dddd, MMMM dd, yyyy"))
                            .SetBold()
                            .SetFontColor(new DeviceRgb(99, 102, 241)));

                        document.Add(new Paragraph(entry.Title ?? "Untitled")
                            .SetBold()
                            .SetFontSize(14));

                        document.Add(new Paragraph(entry.Content ?? string.Empty));

                        document.Add(new Paragraph($"Word Count: {entry.WordCount}")
                            .SetFontSize(10)
                            .SetFontColor(ColorConstants.GRAY));

                        document.Add(new Paragraph(" "));
                    }
                }
                if (File.Exists(filePath))
                {
                    Console.WriteLine($"[ExportService] PDF file successfully created at: {filePath}");
                }
                else
                {
                    Console.WriteLine($"[ExportService] PDF file was NOT created at: {filePath}");
                }
                return $"Successfully exported {entries.Count} entries as PDF. Path: {filePath}";
            }
            catch (Exception ex)
            {
                var detail = ex.InnerException != null ? $" | Inner: {ex.InnerException.Message}" : "";
                return $"Error creating export: {ex.GetType().Name}: {ex.Message}{detail}";
            }
        }

        
    }
}
