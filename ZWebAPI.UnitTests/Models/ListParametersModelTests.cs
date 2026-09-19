using ZWebAPI.Interfaces;
using ZWebAPI.Models;

namespace ZWebAPI.UnitTests.Models
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.ListParametersModel"/>.
    /// </summary>
    public class ListParametersModelTests
    {
        /// <summary>
        /// Test the model should start with an empty pagination and a case insensitive sort dictionary.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartWithoutPaginationAndWithACaseInsensitiveSort()
        {
            // Arrange & Act
            ListParametersModel parameters = new();

            // Assert
            parameters.StartRow.Should().Be(0);
            parameters.EndRow.Should().Be(0);
            parameters.Sort.Should().BeEmpty();
            parameters.Filters.Should().BeEmpty();

            parameters.Sort["Name"] = "asc";
            parameters.Sort.Should().ContainKey("NAME");
        }

        /// <summary>
        /// Test the model should expose the list contract with every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeTheListContract()
        {
            // Arrange
            Dictionary<string, string> sort = new() { ["Name"] = "desc" };

            // Act
            IListParameters parameters = new ListParametersModel
            {
                StartRow = 10,
                EndRow = 20,
                Sort = sort,
            };

            // Assert
            parameters.StartRow.Should().Be(10);
            parameters.EndRow.Should().Be(20);
            parameters.Sort.Should().BeSameAs(sort);
        }
    }
}
