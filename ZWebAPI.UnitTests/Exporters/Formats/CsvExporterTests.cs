using System.Text;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Formats;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters.Formats
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Formats.CsvExporter"/>.
    /// </summary>
    public class CsvExporterTests
    {
        /// <summary>
        /// Test the Export should describe the payload as a UTF-8 CSV file.
        /// </summary>
        [Fact]
        public void Export_Pass_DescribesThePayloadAsCsv()
        {
            // Arrange
            CsvExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.ContentType.Should().Be("text/csv; charset=utf-8");
            result.FileName.Should().Be("report.csv");
        }

        /// <summary>
        /// Test the Export should prefix the payload with the UTF-8 byte order mark.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesTheUtf8ByteOrderMark()
        {
            // Arrange
            CsvExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.Content.Take(3).Should().Equal(Encoding.UTF8.GetPreamble());
        }

        /// <summary>
        /// Test the Export should write a comma separated header row terminated with CRLF.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesTheHeaderRow()
        {
            // Arrange
            CsvExporter exporter = new();
            IReadOnlyList<ExportColumn> columns =
            [
                new("First", ExportColumnType.Text, row => row),
                new("Second", ExportColumnType.Text, row => row),
            ];

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            ReadBody(result).Should().Be("First,Second\r\n");
        }

        /// <summary>
        /// Test the Export should write one formatted line per row.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesOneFormattedLinePerRow()
        {
            // Arrange
            CsvExporter exporter = new();
            List<object> rows = [new ExportRowFake { ID = 1, Name = "First", IsActive = true }, new ExportRowFake { ID = 2, Name = "Second" }];
            IReadOnlyList<ExportColumn> columns =
            [
                new("ID", ExportColumnType.Text, row => ((ExportRowFake)row).ID),
                new("Active", ExportColumnType.Boolean, row => ((ExportRowFake)row).IsActive),
            ];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            ReadBody(result).Should().Be("ID,Active\r\n1,Sim\r\n2,Não\r\n");
        }

        /// <summary>
        /// Test the Export should quote and escape the values that carry CSV control characters.
        /// </summary>
        /// <param name="value">The raw value.</param>
        /// <param name="expected">The expected escaped rendering.</param>
        [Theory]
        [InlineData("plain", "plain")]
        [InlineData("with,comma", "\"with,comma\"")]
        [InlineData("with\"quote", "\"with\"\"quote\"")]
        [InlineData("with\rreturn", "\"with\rreturn\"")]
        [InlineData("with\nfeed", "\"with\nfeed\"")]
        public void Export_Pass_EscapesControlCharacters(string value, string expected)
        {
            // Arrange
            CsvExporter exporter = new();
            List<object> rows = [value];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            ReadBody(result).Should().Be($"Value\r\n{expected}\r\n");
        }

        /// <summary>
        /// Test the Export should escape a header carrying CSV control characters.
        /// </summary>
        [Fact]
        public void Export_Pass_EscapesTheHeader()
        {
            // Arrange
            CsvExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text, "Header,with comma");

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            ReadBody(result).Should().Be("\"Header,with comma\"\r\n");
        }

        /// <summary>
        /// Test the Export should render an empty cell when the value is null.
        /// </summary>
        [Fact]
        public void Export_Pass_RendersEmptyCellForNullValue()
        {
            // Arrange
            CsvExporter exporter = new();
            List<object> rows = [ExportColumnFakeFactory.NullValueRow()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            ReadBody(result).Should().Be("Value\r\n\r\n");
        }

        /// <summary>
        /// Test the Export should produce blank lines when there is no column at all.
        /// </summary>
        [Fact]
        public void Export_Pass_ProducesBlankLinesWithoutColumns()
        {
            // Arrange
            CsvExporter exporter = new();
            List<object> rows = ["ignored"];
            IReadOnlyList<ExportColumn> columns = [];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            ReadBody(result).Should().Be("\r\n\r\n");
        }

        private static string ReadBody(ExportResult result)
        {
            return Encoding.UTF8.GetString(result.Content, Encoding.UTF8.GetPreamble().Length, result.Content.Length - Encoding.UTF8.GetPreamble().Length);
        }
    }
}
