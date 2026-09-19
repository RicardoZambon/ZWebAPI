using ZWebAPI.Interfaces;
using ZWebAPI.Models;

namespace ZWebAPI.UnitTests.Models
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.CatalogParametersModel"/>.
    /// </summary>
    public class CatalogParametersModelTests
    {
        /// <summary>
        /// Test the model should start without a criteria and without a result cap.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartWithoutCriteriaAndWithoutCap()
        {
            // Arrange & Act
            CatalogParametersModel parameters = new();

            // Assert
            parameters.Criteria.Should().BeNull();
            parameters.MaxResults.Should().Be(0);
            parameters.Filters.Should().BeEmpty();
        }

        /// <summary>
        /// Test the model should expose the catalog contract with every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeTheCatalogContract()
        {
            // Arrange & Act
            ICatalogParameters parameters = new CatalogParametersModel
            {
                Criteria = "abc",
                MaxResults = 25,
            };

            // Assert
            parameters.Criteria.Should().Be("abc");
            parameters.MaxResults.Should().Be(25);
        }
    }
}
