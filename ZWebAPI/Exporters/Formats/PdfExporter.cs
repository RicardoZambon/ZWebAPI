using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;

namespace ZWebAPI.Exporters.Formats
{
    /// <summary>
    /// PDF exporter using QuestPDF. Produces a landscape A4 document with a repeating header row,
    /// auto-sized columns, and alternating row shading for readability.
    /// </summary>
    internal sealed class PdfExporter
    {
        #region Constructors
        /// <summary>
        /// Configures the QuestPDF license once per process. Runs on first load of <see cref="PdfExporter"/>,
        /// which covers every code path that actually renders a PDF (services, tests, and the WebApi host).
        /// </summary>
        static PdfExporter()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }
        #endregion

        #region Public methods
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, string fileBaseName)
        {
            byte[] bytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4.Landscape());
                    page.Margin(20);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Content().Table(table =>
                    {
                        table.ColumnsDefinition(cd =>
                        {
                            for (int i = 0; i < columns.Count; i++)
                            {
                                cd.RelativeColumn();
                            }
                        });

                        table.Header(header =>
                        {
                            foreach (ExportColumn column in columns)
                            {
                                header.Cell().Background(Colors.Grey.Lighten2)
                                    .Padding(4)
                                    .Text(column.Header)
                                    .Bold();
                            }
                        });

                        int rowIndex = 0;
                        foreach (object row in rows)
                        {
                            string background = rowIndex % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                            for (int c = 0; c < columns.Count; c++)
                            {
                                object? raw = columns[c].ValueSelector(row);
                                string formatted = ExportValueFormatter.Format(raw, columns[c].Type);
                                table.Cell().Background(background).Padding(4).Text(formatted);
                            }
                            rowIndex++;
                        }
                    });

                    page.Footer().AlignRight().Text(x =>
                    {
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            }).GeneratePdf();

            return new ExportResult(bytes, "application/pdf", $"{fileBaseName}.pdf");
        }
        #endregion
    }
}
