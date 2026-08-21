using Documents.Application.Common.Dtos;
using Documents.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Documents.Infrastructure.Pdf;

public sealed class AppointmentResultPdfRenderer : IPdfRenderer
{
    private readonly PdfSettings _settings;

    public AppointmentResultPdfRenderer(IOptions<PdfSettings> settings)
    {
        _settings = settings.Value;
    }

    public byte[] RenderAppointmentResult(AppointmentResultDocumentDto result)
        => Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(style => style.FontFamily(_settings.FontFamily).FontSize(11));

                page.Header().Element(header => ComposeHeader(header, result));
                page.Content().Element(content => ComposeContent(content, result));
                page.Footer().Element(ComposeFooter);
            });
        }).GeneratePdf();

    private void ComposeHeader(IContainer container, AppointmentResultDocumentDto result)
    {
        container.Column(column =>
        {
            column.Item().Text(_settings.ClinicName).FontSize(16).Bold();
            column.Item().PaddingTop(2).Text("Результат приёма").FontSize(13);
            column.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private static void ComposeContent(IContainer container, AppointmentResultDocumentDto result)
    {
        container.PaddingVertical(14).Column(column =>
        {
            column.Spacing(12);

            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(160);
                    columns.RelativeColumn();
                });

                AddRow(table, "Дата приёма", FormatSchedule(result));
                AddRow(table, "Пациент", result.PatientFullName);
                AddRow(table, "Дата рождения", result.PatientDateOfBirth.ToString("dd.MM.yyyy"));
                AddRow(table, "Врач", result.DoctorFullName);
                AddRow(table, "Специализация", result.SpecializationName);
                AddRow(table, "Услуга", result.ServiceName);
            });

            AddSection(column, "Жалобы", result.Complaints);
            AddSection(column, "Заключение", result.Conclusion);
            AddSection(column, "Диагноз", result.Diagnosis);
            AddSection(column, "Рекомендации", result.Recommendations);
        });
    }

    private static void ComposeFooter(IContainer container)
    {
        container.AlignCenter().Text(text =>
        {
            text.DefaultTextStyle(style => style.FontSize(9).FontColor(Colors.Grey.Darken1));
            text.CurrentPageNumber();
            text.Span(" / ");
            text.TotalPages();
        });
    }

    private static void AddRow(TableDescriptor table, string label, string value)
    {
        table.Cell().Element(CellStyle).Text(label).Bold();
        table.Cell().Element(CellStyle).Text(value);
    }

    private static void AddSection(ColumnDescriptor column, string title, string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return;

        column.Item().Column(section =>
        {
            section.Item().Text(title).Bold();
            section.Item().PaddingTop(3).Text(text);
        });
    }

    private static IContainer CellStyle(IContainer container)
        => container
            .BorderBottom(1)
            .BorderColor(Colors.Grey.Lighten2)
            .PaddingVertical(5)
            .PaddingRight(8);

    private static string FormatSchedule(AppointmentResultDocumentDto result)
        => $"{result.Date:dd.MM.yyyy}, {result.StartTime.ToString("HH\\:mm")}–{result.EndTime.ToString("HH\\:mm")}";
}
