using ClosedXML.Excel;
using JuniorCodeCRM.Helpers;
using JuniorCodeCRM.Models.Enums;
using JuniorCodeCRM.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JuniorCodeCRM.Services;

public class ExportService : IExportService
{
    private readonly IReportService _reportService;

    public ExportService(IReportService reportService)
    {
        _reportService = reportService;
    }

    public async Task<byte[]> ExportStaffByDepartmentAsync(ExportFormat format)
    {
        var data = await _reportService.GetStaffByDepartmentReportAsync();

        return format == ExportFormat.XLSX
            ? ExportToXlsx(data, "Кадровый состав по отделам")
            : ExportToPdf(data, "Кадровый состав по отделам");
    }



    public async Task<byte[]> ExportTaskExecutionAsync(ExportFormat format)
    {
        var data = await _reportService.GetTaskExecutionReportAsync();

        return format == ExportFormat.XLSX
            ? ExportToXlsx(data, "Исполнение поручений")
            : ExportToPdf(data, "Исполнение поручений");
    }

    public async Task<byte[]> ExportTeacherLoadAsync(ExportFormat format)
    {
        var data = await _reportService.GetTeacherLoadReportAsync();

        return format == ExportFormat.XLSX
            ? ExportToXlsx(data, "Загрузка преподавателей")
            : ExportToPdf(data, "Загрузка преподавателей");
    }

    /// <summary>
    /// Экспорт в Excel (XLSX) с помощью ClosedXML
    /// </summary>
    private byte[] ExportToXlsx<T>(List<T> data, string reportName)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(reportName);

        // Заголовок
        worksheet.Cell(1, 1).Value = reportName;
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;

        // Дата формирования
        worksheet.Cell(2, 1).Value = $"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}";

        // Данные
        worksheet.Cell(4, 1).InsertTable(data);
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    /// <summary>
    /// Экспорт в PDF с помощью QuestPDF
    /// Заглушка — возвращает базовый PDF
    /// </summary>
    private byte[] ExportToPdf<T>(List<T> data, string reportName)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(30);
                page.DefaultTextStyle(x => x.FontSize(10));

                page.Header().Element(c =>
                {
                    c.Column(col =>
                    {
                        col.Item().Text(reportName)
                            .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);
                        col.Item().Text($"Дата формирования: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .FontSize(9).FontColor(Colors.Grey.Medium);
                        col.Item().PaddingVertical(5);
                    });
                });

                page.Content().Element(c =>
                {
                    c.Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            var properties = typeof(T).GetProperties();
                            foreach (var _ in properties)
                                columns.RelativeColumn();
                        });

                        // Заголовки
                        table.Header(header =>
                        {
                            var properties = typeof(T).GetProperties();
                            foreach (var prop in properties)
                            {
                                header.Cell().Background(Colors.Grey.Lighten3)
                                    .Padding(4).Text(prop.Name).Bold().FontSize(9);
                            }
                        });

                        // Данные
                        foreach (var item in data)
                        {
                            var properties = typeof(T).GetProperties();
                            foreach (var prop in properties)
                            {
                                var value = prop.GetValue(item)?.ToString() ?? "";
                                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten4)
                                    .Padding(4).Text(value).FontSize(9);
                            }
                        }
                    });
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Страница ");
                    x.CurrentPageNumber();
                    x.Span(" из ");
                    x.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}