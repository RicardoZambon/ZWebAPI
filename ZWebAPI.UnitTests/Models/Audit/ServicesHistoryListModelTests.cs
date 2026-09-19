using ZWebAPI.Models.Audit.ServiceHistory;

namespace ZWebAPI.UnitTests.Models.Audit
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.Audit.ServiceHistory.ServicesHistoryListModel"/>.
    /// </summary>
    public class ServicesHistoryListModelTests
    {
        /// <summary>
        /// Test the model should start empty.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartEmpty()
        {
            // Arrange & Act
            ServicesHistoryListModel model = new();

            // Assert
            model.ChangedOn.Should().Be(default);
            model.ChangedByName.Should().BeNull();
            model.ID.Should().Be(0);
            model.Name.Should().BeNull();
        }

        /// <summary>
        /// Test the model should expose every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeEveryValue()
        {
            // Arrange
            DateTime changedOn = new(2024, 3, 9, 21, 45, 0);

            // Act
            ServicesHistoryListModel model = new()
            {
                ChangedOn = changedOn,
                ChangedByName = "User",
                ID = 7,
                Name = "IService\\Method",
            };

            // Assert
            model.ChangedOn.Should().Be(changedOn);
            model.ChangedByName.Should().Be("User");
            model.ID.Should().Be(7);
            model.Name.Should().Be("IService\\Method");
        }
    }
}
