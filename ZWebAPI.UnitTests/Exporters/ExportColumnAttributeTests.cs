using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ExportColumnAttribute"/>.
    /// </summary>
    public class ExportColumnAttributeTests
    {
        /// <summary>
        /// Test the parameterless constructor should leave every override at its default.
        /// </summary>
        [Fact]
        public void Constructor_Pass_LeavesDefaultsWhenNoArgumentIsProvided()
        {
            // Arrange & Act
            ExportColumnAttribute attribute = new();

            // Assert
            attribute.Header.Should().BeNull();
            attribute.Ignore.Should().BeFalse();
            attribute.Order.Should().Be(int.MaxValue);
            attribute.Type.Should().Be(ExportColumnType.Text);
        }

        /// <summary>
        /// Test the header constructor should store the provided header.
        /// </summary>
        [Fact]
        public void Constructor_Pass_StoresTheProvidedHeader()
        {
            // Arrange
            string header = "Custom";

            // Act
            ExportColumnAttribute attribute = new(header);

            // Assert
            attribute.Header.Should().Be(header);
            attribute.Ignore.Should().BeFalse();
            attribute.Order.Should().Be(int.MaxValue);
            attribute.Type.Should().Be(ExportColumnType.Text);
        }

        /// <summary>
        /// Test every override should be settable through the object initializer.
        /// </summary>
        [Fact]
        public void Properties_Pass_AcceptEveryOverride()
        {
            // Arrange & Act
            ExportColumnAttribute attribute = new()
            {
                Header = "Header",
                Ignore = true,
                Order = 3,
                Type = ExportColumnType.Date,
            };

            // Assert
            attribute.Header.Should().Be("Header");
            attribute.Ignore.Should().BeTrue();
            attribute.Order.Should().Be(3);
            attribute.Type.Should().Be(ExportColumnType.Date);
        }
    }
}
