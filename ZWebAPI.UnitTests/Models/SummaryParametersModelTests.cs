using System.Text.Json;
using ZWebAPI.Models;

namespace ZWebAPI.UnitTests.Models
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.SummaryParametersModel"/>.
    /// </summary>
    public class SummaryParametersModelTests
    {
        /// <summary>
        /// Test the Filters should start as an empty case insensitive dictionary.
        /// </summary>
        [Fact]
        public void Filters_Pass_StartAsAnEmptyCaseInsensitiveDictionary()
        {
            // Arrange & Act
            SummaryParametersModel parameters = new();

            // Assert
            parameters.Filters.Should().NotBeNull();
            parameters.Filters.Should().BeEmpty();

            parameters.Filters!["Name"] = JsonDocument.Parse("\"abc\"").RootElement;
            parameters.Filters.Should().ContainKey("NAME");
        }

        /// <summary>
        /// Test the Filters should accept a replacement dictionary, including a null one.
        /// </summary>
        [Fact]
        public void Filters_Pass_AcceptAReplacementDictionary()
        {
            // Arrange
            SummaryParametersModel parameters = new();
            Dictionary<string, object> filters = new() { ["Name"] = 1 };

            // Act
            parameters.Filters = filters;

            // Assert
            parameters.Filters.Should().BeSameAs(filters);

            parameters.Filters = null;
            parameters.Filters.Should().BeNull();
        }
    }
}
