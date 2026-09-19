using System.Text;
using System.Xml.Linq;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Formats;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters.Formats
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Formats.XmlExporter"/>.
    /// </summary>
    public class XmlExporterTests
    {
        /// <summary>
        /// Test the Export should describe the payload as a UTF-8 XML file.
        /// </summary>
        [Fact]
        public void Export_Pass_DescribesThePayloadAsXml()
        {
            // Arrange
            XmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.ContentType.Should().Be("application/xml; charset=utf-8");
            result.FileName.Should().Be("report.xml");
        }

        /// <summary>
        /// Test the Export should omit the byte order mark so the declaration starts the document.
        /// </summary>
        [Fact]
        public void Export_Pass_OmitsTheByteOrderMark()
        {
            // Arrange
            XmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            Encoding.UTF8.GetString(result.Content).Should().StartWith("<?xml");
        }

        /// <summary>
        /// Test the Export should emit an empty rows element when there is nothing to write.
        /// </summary>
        [Fact]
        public void Export_Pass_EmitsAnEmptyDocumentForNoRows()
        {
            // Arrange
            XmlExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            XDocument document = XDocument.Parse(Encoding.UTF8.GetString(result.Content));
            document.Root!.Name.LocalName.Should().Be("rows");
            document.Root.Elements().Should().BeEmpty();
        }

        /// <summary>
        /// Test the Export should emit one row element per row and one column element per column.
        /// </summary>
        [Fact]
        public void Export_Pass_EmitsOneColumnElementPerColumn()
        {
            // Arrange
            XmlExporter exporter = new();
            List<object> rows =
            [
                new ExportRowFake { ID = 1, Name = "First", IsActive = true },
                new ExportRowFake { ID = 2, Name = "Second" },
            ];
            IReadOnlyList<ExportColumn> columns =
            [
                new("Identifier", ExportColumnType.Text, row => ((ExportRowFake)row).ID),
                new("Active", ExportColumnType.Boolean, row => ((ExportRowFake)row).IsActive),
            ];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            XDocument document = XDocument.Parse(Encoding.UTF8.GetString(result.Content));
            List<XElement> rowElements = [.. document.Root!.Elements("row")];

            rowElements.Should().HaveCount(2);
            rowElements[0].Elements("column").Select(x => x.Attribute("name")!.Value).Should().ContainInOrder("Identifier", "Active");
            rowElements[0].Elements("column").Select(x => x.Value).Should().ContainInOrder("1", "Sim");
            rowElements[1].Elements("column").Select(x => x.Value).Should().ContainInOrder("2", "Não");
        }

        /// <summary>
        /// Test the Export should escape markup characters found in values and headers.
        /// </summary>
        [Fact]
        public void Export_Pass_EscapesMarkupCharacters()
        {
            // Arrange
            XmlExporter exporter = new();
            List<object> rows = ["<value> & \"quoted\""];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text, "Header & <tag>");

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            XDocument document = XDocument.Parse(Encoding.UTF8.GetString(result.Content));
            XElement column = document.Root!.Element("row")!.Element("column")!;

            column.Attribute("name")!.Value.Should().Be("Header & <tag>");
            column.Value.Should().Be("<value> & \"quoted\"");
        }

        /// <summary>
        /// Test the Export should write an empty column element when the value is null.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesEmptyColumnForNullValue()
        {
            // Arrange
            XmlExporter exporter = new();
            List<object> rows = [ExportColumnFakeFactory.NullValueRow()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            XDocument document = XDocument.Parse(Encoding.UTF8.GetString(result.Content));
            document.Root!.Element("row")!.Element("column")!.Value.Should().BeEmpty();
        }

        /// <summary>
        /// Test the Export should emit an empty row element when there is no column.
        /// </summary>
        [Fact]
        public void Export_Pass_EmitsEmptyRowWithoutColumns()
        {
            // Arrange
            XmlExporter exporter = new();
            List<object> rows = ["ignored"];
            IReadOnlyList<ExportColumn> columns = [];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            XDocument document = XDocument.Parse(Encoding.UTF8.GetString(result.Content));
            document.Root!.Elements("row").Should().ContainSingle();
            document.Root.Element("row")!.Elements().Should().BeEmpty();
        }
    }
}
