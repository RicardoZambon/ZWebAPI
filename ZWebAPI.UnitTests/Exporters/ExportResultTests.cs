using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ExportResult"/>.
    /// </summary>
    public class ExportResultTests
    {
        /// <summary>
        /// Test the constructor should expose every value it received.
        /// </summary>
        [Fact]
        public void Constructor_Pass_ExposesTheProvidedPayload()
        {
            // Arrange
            byte[] content = [1, 2, 3];
            string contentType = "text/csv";
            string fileName = "file.csv";

            // Act
            ExportResult result = new(content, contentType, fileName);

            // Assert
            result.Content.Should().BeSameAs(content);
            result.ContentType.Should().Be(contentType);
            result.FileName.Should().Be(fileName);
        }
    }
}
