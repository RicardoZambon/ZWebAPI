using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ExportColumn"/>.
    /// </summary>
    public class ExportColumnTests
    {
        /// <summary>
        /// Test the constructor should expose every value it received.
        /// </summary>
        [Fact]
        public void Constructor_Pass_ExposesTheProvidedMetadata()
        {
            // Arrange
            string header = "Header";
            ExportColumnType type = ExportColumnType.Currency;
            Func<object, object?> selector = row => row.ToString();

            // Act
            ExportColumn column = new(header, type, selector);

            // Assert
            column.Header.Should().Be(header);
            column.Type.Should().Be(type);
            column.ValueSelector.Should().BeSameAs(selector);
            column.ValueSelector(12).Should().Be("12");
        }
    }
}
