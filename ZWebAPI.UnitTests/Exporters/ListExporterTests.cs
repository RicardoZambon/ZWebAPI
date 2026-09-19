using System.Text;
using ZWebAPI.Exporters;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ListExporter"/>.
    /// </summary>
    public class ListExporterTests
    {
        /// <summary>
        /// Test the generic Export should reflect the columns from the row type.
        /// </summary>
        [Fact]
        public void Export_Pass_ReflectsColumnsFromTheRowType()
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            List<ExportRowFake> rows = [new() { ID = 1, Name = "First", Secret = "hidden" }];

            // Act
            ExportResult result = exporter.Export(rows, ExportFormat.Csv, "rows");

            // Assert
            string csv = Encoding.UTF8.GetString(result.Content);
            csv.Should().Contain("Full name");
            csv.Should().Contain("First");
            csv.Should().NotContain("Secret");
            csv.Should().NotContain("hidden");
        }

        /// <summary>
        /// Test the generic Export should produce a header-only payload for an empty row collection.
        /// </summary>
        [Fact]
        public void Export_Pass_ProducesHeaderOnlyPayloadForEmptyRows()
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            List<ExportRowFake> rows = [];

            // Act
            ExportResult result = exporter.Export(rows, ExportFormat.Csv, "rows");

            // Assert
            string csv = Encoding.UTF8.GetString(result.Content);
            csv.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Should().ContainSingle();
        }

        /// <summary>
        /// Test the Export should dispatch to the exporter matching the requested format.
        /// </summary>
        /// <param name="format">The requested format.</param>
        /// <param name="expectedContentType">The content type the matching exporter produces.</param>
        /// <param name="expectedExtension">The extension the matching exporter appends.</param>
        [Theory]
        [InlineData(ExportFormat.Csv, "text/csv; charset=utf-8", ".csv")]
        [InlineData(ExportFormat.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", ".xlsx")]
        [InlineData(ExportFormat.Pdf, "application/pdf", ".pdf")]
        [InlineData(ExportFormat.Xml, "application/xml; charset=utf-8", ".xml")]
        [InlineData(ExportFormat.Mhtml, "message/rfc822", ".mhtml")]
        public void Export_Pass_DispatchesToTheFormatExporter(ExportFormat format, string expectedContentType, string expectedExtension)
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            List<ExportRowFake> rows = [new() { ID = 1, Name = "First" }];

            // Act
            ExportResult result = exporter.Export(rows, format, "report");

            // Assert
            result.ContentType.Should().Be(expectedContentType);
            result.FileName.Should().Be($"report{expectedExtension}");
            result.Content.Should().NotBeEmpty();
        }

        /// <summary>
        /// Test the Export should throw when the row collection is null.
        /// </summary>
        [Fact]
        public void Export_Fail_ThrowsWhenRowsAreNull()
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            Action act = () => exporter.Export(null!, columns, ExportFormat.Csv, "rows");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("rows");
        }

        /// <summary>
        /// Test the Export should throw when the column metadata is null.
        /// </summary>
        [Fact]
        public void Export_Fail_ThrowsWhenColumnsAreNull()
        {
            // Arrange
            IListExporter exporter = new ListExporter();

            // Act
            Action act = () => exporter.Export([], null!, ExportFormat.Csv, "rows");

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("columns");
        }

        /// <summary>
        /// Test the Export should throw when the file base name is missing.
        /// </summary>
        /// <param name="fileBaseName">The invalid file base name.</param>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Export_Fail_ThrowsWhenFileBaseNameIsMissing(string? fileBaseName)
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            Action act = () => exporter.Export([], columns, ExportFormat.Csv, fileBaseName!);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithParameterName("fileBaseName");
        }

        /// <summary>
        /// Test the Export should throw when the requested format is not supported.
        /// </summary>
        [Fact]
        public void Export_Fail_ThrowsWhenFormatIsNotSupported()
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            Action act = () => exporter.Export([], columns, (ExportFormat)99, "rows");

            // Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
                .WithParameterName("format")
                .WithMessage("*Unsupported export format.*");
        }

        /// <summary>
        /// Test the Export should honor explicitly provided columns instead of reflecting the row type.
        /// </summary>
        [Fact]
        public void Export_Pass_HonorsExplicitlyProvidedColumns()
        {
            // Arrange
            IListExporter exporter = new ListExporter();
            List<object> rows = [new ExportRowFake { ID = 1, Name = "First", Secret = "visible now" }];
            IReadOnlyList<ExportColumn> columns =
            [
                new("Only", ExportColumnType.Text, row => ((ExportRowFake)row).Secret),
            ];

            // Act
            ExportResult result = exporter.Export(rows, columns, ExportFormat.Csv, "rows");

            // Assert
            string csv = Encoding.UTF8.GetString(result.Content);
            csv.Should().Contain("Only");
            csv.Should().Contain("visible now");
            csv.Should().NotContain("Full name");
        }
    }
}
