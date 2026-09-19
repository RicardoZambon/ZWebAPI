using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Exceptions;

namespace ZWebAPI.UnitTests.Exporters.Exceptions
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.Exceptions.ExportRowLimitExceededException"/>.
    /// </summary>
    public class ExportRowLimitExceededExceptionTests
    {
        /// <summary>
        /// Test the constructor should expose the configured limit and describe it in the message.
        /// </summary>
        [Fact]
        public void Constructor_Pass_ExposesTheConfiguredLimit()
        {
            // Arrange
            int maxRows = 1234;

            // Act
            ExportRowLimitExceededException exception = new(maxRows);

            // Assert
            exception.MaxRows.Should().Be(maxRows);
            exception.Message.Should().Be($"The export result exceeds the configured limit of {maxRows} rows.");
        }

        /// <summary>
        /// Test the exception should carry the shared export row limit when raised by the library.
        /// </summary>
        [Fact]
        public void Constructor_Pass_CarriesTheSharedRowLimit()
        {
            // Arrange & Act
            ExportRowLimitExceededException exception = new(ExportConstants.MaxExportRows);

            // Assert
            ExportConstants.MaxExportRows.Should().Be(50_000);
            exception.MaxRows.Should().Be(50_000);
            exception.Should().BeAssignableTo<Exception>();
        }
    }
}
