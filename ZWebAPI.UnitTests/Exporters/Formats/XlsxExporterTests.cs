using ClosedXML.Excel;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Formats;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters.Formats
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Formats.XlsxExporter"/>.
    /// </summary>
    public class XlsxExporterTests
    {
        /// <summary>
        /// Test the Export should describe the payload as an Excel workbook.
        /// </summary>
        [Fact]
        public void Export_Pass_DescribesThePayloadAsWorkbook()
        {
            // Arrange
            XlsxExporter exporter = new();
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            result.ContentType.Should().Be("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            result.FileName.Should().Be("report.xlsx");
        }

        /// <summary>
        /// Test the Export should write a bold header row into a frozen sheet named Export.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesABoldFrozenHeaderRow()
        {
            // Arrange
            XlsxExporter exporter = new();
            IReadOnlyList<ExportColumn> columns =
            [
                new("First", ExportColumnType.Text, row => row),
                new("Second", ExportColumnType.Text, row => row),
            ];

            // Act
            ExportResult result = exporter.Export([], columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Name.Should().Be("Export");
            sheet.Cell(1, 1).GetString().Should().Be("First");
            sheet.Cell(1, 2).GetString().Should().Be("Second");
            sheet.Cell(1, 1).Style.Font.Bold.Should().BeTrue();
            sheet.SheetView.SplitRow.Should().Be(1);
        }

        /// <summary>
        /// Test the Export should write one sheet row per exported row starting below the header.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesOneSheetRowPerExportedRow()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = ["first", "second", "third"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be("first");
            sheet.Cell(3, 1).GetString().Should().Be("second");
            sheet.Cell(4, 1).GetString().Should().Be("third");
            sheet.Cell(5, 1).Value.IsBlank.Should().BeTrue();
        }

        /// <summary>
        /// Test the Export should leave the cell blank when the value is null.
        /// </summary>
        [Fact]
        public void Export_Pass_LeavesTheCellBlankForNullValue()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [ExportColumnFakeFactory.NullValueRow()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Number);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).Value.IsBlank.Should().BeTrue();
        }

        /// <summary>
        /// Test the Export should write a native date value with the date format.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesNativeDateValues()
        {
            // Arrange
            XlsxExporter exporter = new();
            DateTime value = new(2024, 3, 9, 21, 45, 0);
            List<object> rows = [value];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Date);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetDateTime().Should().Be(value);
            sheet.Cell(2, 1).Style.DateFormat.Format.Should().Be("dd/MM/yyyy");
        }

        /// <summary>
        /// Test the Export should write a native date and time value with the date and time format.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesNativeDateTimeValues()
        {
            // Arrange
            XlsxExporter exporter = new();
            DateTime value = new(2024, 3, 9, 21, 45, 0);
            List<object> rows = [value];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.DateTime);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetDateTime().Should().Be(value);
            sheet.Cell(2, 1).Style.DateFormat.Format.Should().Be("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Test the Export should fall back to text when a date column holds another type.
        /// </summary>
        /// <param name="type">The date-like column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Date)]
        [InlineData(ExportColumnType.DateTime)]
        public void Export_Pass_FallsBackToTextWhenDateColumnHoldsAnotherType(ExportColumnType type)
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = ["not a date"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(type);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be("not a date");
        }

        /// <summary>
        /// Test the Export should write a native currency value with the currency format.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesNativeCurrencyValues()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [1234.5m];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Currency);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetDouble().Should().Be(1234.5);
            sheet.Cell(2, 1).Style.NumberFormat.Format.Should().Be("\"R$\" #,##0.00");
        }

        /// <summary>
        /// Test the Export should write a native number value.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesNativeNumberValues()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [1234.5m];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Number);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetDouble().Should().Be(1234.5);
        }

        /// <summary>
        /// Test the Export should fall back to text when a numeric column holds a non-convertible value.
        /// </summary>
        /// <param name="type">The numeric column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Currency)]
        [InlineData(ExportColumnType.Number)]
        public void Export_Pass_FallsBackToTextWhenNumericColumnHoldsNonConvertible(ExportColumnType type)
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [new NonFormattableFake()];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(type);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be(NonFormattableFake.Text);
        }

        /// <summary>
        /// Test the Export should write booleans in Portuguese.
        /// </summary>
        /// <param name="value">The boolean value under test.</param>
        /// <param name="expected">The expected rendering.</param>
        [Theory]
        [InlineData(true, "Sim")]
        [InlineData(false, "Não")]
        public void Export_Pass_WritesBooleansInPortuguese(bool value, string expected)
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [value];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Boolean);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be(expected);
        }

        /// <summary>
        /// Test the Export should fall back to text when a boolean column holds another type.
        /// </summary>
        [Fact]
        public void Export_Pass_FallsBackToTextWhenBooleanColumnHoldsAnotherType()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = ["maybe"];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Boolean);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be("maybe");
        }

        /// <summary>
        /// Test the Export should write text columns with the plain string representation.
        /// </summary>
        [Fact]
        public void Export_Pass_WritesTextColumnsAsStrings()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = [42];
            IReadOnlyList<ExportColumn> columns = ExportColumnFakeFactory.Identity(ExportColumnType.Text);

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(2, 1).GetString().Should().Be("42");
        }

        /// <summary>
        /// Test the Export should produce a workbook with only the header when there is no column.
        /// </summary>
        [Fact]
        public void Export_Pass_ProducesAWorkbookWithoutColumns()
        {
            // Arrange
            XlsxExporter exporter = new();
            List<object> rows = ["ignored"];
            IReadOnlyList<ExportColumn> columns = [];

            // Act
            ExportResult result = exporter.Export(rows, columns, "report");

            // Assert
            IXLWorksheet sheet = ReadSheet(result);
            sheet.Cell(1, 1).Value.IsBlank.Should().BeTrue();
        }

        private static IXLWorksheet ReadSheet(ExportResult result)
        {
            MemoryStream stream = new(result.Content);
            XLWorkbook workbook = new(stream);
            return workbook.Worksheet(1);
        }
    }
}
