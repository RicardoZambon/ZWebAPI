using ZWebAPI.Exporters;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ExportValueFormatter"/>.
    /// </summary>
    public class ExportValueFormatterTests
    {
        /// <summary>
        /// Test the Culture should be the Brazilian Portuguese culture used across every exporter.
        /// </summary>
        [Fact]
        public void Culture_Pass_IsBrazilianPortuguese()
        {
            // Arrange & Act
            string name = ExportValueFormatter.Culture.Name;

            // Assert
            name.Should().Be("pt-BR");
        }

        /// <summary>
        /// Test the Format should return an empty string for a null value, whatever the column type.
        /// </summary>
        /// <param name="type">The column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Text)]
        [InlineData(ExportColumnType.Number)]
        [InlineData(ExportColumnType.Date)]
        [InlineData(ExportColumnType.DateTime)]
        [InlineData(ExportColumnType.Currency)]
        [InlineData(ExportColumnType.Boolean)]
        public void Format_Pass_ReturnsEmptyForNullValue(ExportColumnType type)
        {
            // Arrange
            object? value = null;

            // Act
            string result = ExportValueFormatter.Format(value, type);

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Test the Format should render a date without its time component.
        /// </summary>
        [Fact]
        public void Format_Pass_RendersDateWithoutTime()
        {
            // Arrange
            DateTime value = new(2024, 3, 9, 21, 45, 0);

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Date);

            // Assert
            result.Should().Be("09/03/2024");
        }

        /// <summary>
        /// Test the Format should render a date and time value.
        /// </summary>
        [Fact]
        public void Format_Pass_RendersDateTimeWithMinutes()
        {
            // Arrange
            DateTime value = new(2024, 3, 9, 21, 45, 30);

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.DateTime);

            // Assert
            result.Should().Be("09/03/2024 21:45");
        }

        /// <summary>
        /// Test the Format should fall back to the plain string when a date column holds another type.
        /// </summary>
        /// <param name="type">The date-like column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Date)]
        [InlineData(ExportColumnType.DateTime)]
        public void Format_Pass_FallsBackToStringWhenDateColumnHoldsAnotherType(ExportColumnType type)
        {
            // Arrange
            object value = "not a date";

            // Act
            string result = ExportValueFormatter.Format(value, type);

            // Assert
            result.Should().Be("not a date");
        }

        /// <summary>
        /// Test the Format should render currency with the Brazilian symbol and two decimals.
        /// </summary>
        [Fact]
        public void Format_Pass_RendersCurrencyWithTwoDecimals()
        {
            // Arrange
            decimal value = 1234.5m;

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Currency);

            // Assert
            result.Should().StartWith("R$");
            result.Should().EndWith("1.234,50");
        }

        /// <summary>
        /// Test the Format should render numbers with a thousands separator and two decimals.
        /// </summary>
        [Fact]
        public void Format_Pass_RendersNumberWithTwoDecimals()
        {
            // Arrange
            decimal value = 1234.5m;

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Number);

            // Assert
            result.Should().Be("1.234,50");
        }

        /// <summary>
        /// Test the Format should fall back to the plain string when a numeric column holds a non-formattable value.
        /// </summary>
        /// <param name="type">The numeric column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Currency)]
        [InlineData(ExportColumnType.Number)]
        public void Format_Pass_FallsBackToStringWhenNumericColumnHoldsNonFormattable(ExportColumnType type)
        {
            // Arrange
            object value = new NonFormattableFake();

            // Act
            string result = ExportValueFormatter.Format(value, type);

            // Assert
            result.Should().Be(NonFormattableFake.Text);
        }

        /// <summary>
        /// Test the Format should render booleans in Portuguese.
        /// </summary>
        /// <param name="value">The boolean value under test.</param>
        /// <param name="expected">The expected rendering.</param>
        [Theory]
        [InlineData(true, "Sim")]
        [InlineData(false, "Não")]
        public void Format_Pass_RendersBooleanInPortuguese(bool value, string expected)
        {
            // Arrange & Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Boolean);

            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Test the Format should fall back to the plain string when a boolean column holds another type.
        /// </summary>
        [Fact]
        public void Format_Pass_FallsBackToStringWhenBooleanColumnHoldsAnotherType()
        {
            // Arrange
            object value = 1;

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Boolean);

            // Assert
            result.Should().Be("1");
        }

        /// <summary>
        /// Test the Format should render text columns with the plain string representation.
        /// </summary>
        [Fact]
        public void Format_Pass_RendersTextWithPlainString()
        {
            // Arrange
            object value = "plain";

            // Act
            string result = ExportValueFormatter.Format(value, ExportColumnType.Text);

            // Assert
            result.Should().Be("plain");
        }

        /// <summary>
        /// Test the Format should return an empty string when the value renders itself as null.
        /// </summary>
        /// <param name="type">The column type under test.</param>
        [Theory]
        [InlineData(ExportColumnType.Text)]
        [InlineData(ExportColumnType.Number)]
        [InlineData(ExportColumnType.Date)]
        [InlineData(ExportColumnType.DateTime)]
        [InlineData(ExportColumnType.Currency)]
        [InlineData(ExportColumnType.Boolean)]
        public void Format_Pass_ReturnsEmptyWhenValueRendersAsNull(ExportColumnType type)
        {
            // Arrange
            object value = new NullToStringFake();

            // Act
            string result = ExportValueFormatter.Format(value, type);

            // Assert
            result.Should().BeEmpty();
        }
    }
}
