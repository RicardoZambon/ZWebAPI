using System.Text;
using QuestPDF.Infrastructure;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Formats;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters.Formats
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Formats.PdfExporter"/>.
    /// </summary>
    public class PdfExporterTests
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PdfExporterTests"/> class, making sure the
        /// QuestPDF community licence is set before any document is generated.
        /// </summary>
        public PdfExporterTests()
        {
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// Test the Export should describe the payload as a PDF file.
        /// </summary>
        [Fact]
        public void Export_Pass_DescribesThePayloadAsPdf()
        {
            // Arrange
            PdfExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.ContentType.Should().Be("application/pdf");
            result.FileName.Should().Be("report.pdf");
        }

        /// <summary>
        /// Test the Export should produce a document carrying the PDF signature.
        /// </summary>
        [Fact]
        public void Export_Pass_ProducesASignedPdfDocument()
        {
            // Arrange
            PdfExporter exporter = new();
            List<object> rows =
            [
                new ExportRowFake { ID = 1, Name = "First", IsActive = true },
                new ExportRowFake { ID = 2, Name = "Second" },
                new ExportRowFake { ID = 3, Name = "Third" },
            ];
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            result.Content.Should().NotBeEmpty();
            Encoding.ASCII.GetString(result.Content, 0, 5).Should().Be("%PDF-");
        }

        /// <summary>
        /// Test the Export should still produce a document when there is no row.
        /// </summary>
        [Fact]
        public void Export_Pass_ProducesADocumentForNoRows()
        {
            // Arrange
            PdfExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            Encoding.ASCII.GetString(result.Content, 0, 5).Should().Be("%PDF-");
        }

        /// <summary>
        /// Test the Export should render a row whose value is null.
        /// </summary>
        [Fact]
        public void Export_Pass_RendersRowWithNullValue()
        {
            // Arrange
            PdfExporter exporter = new();
            List<object> rows = [ExportColumnFakeFactory.NullValueRow()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            Encoding.ASCII.GetString(result.Content, 0, 5).Should().Be("%PDF-");
        }
    }
}
