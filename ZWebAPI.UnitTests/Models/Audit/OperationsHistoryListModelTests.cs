using ZWebAPI.Models.Audit.OperationHistory;

namespace ZWebAPI.UnitTests.Models.Audit
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.Audit.OperationHistory.OperationsHistoryListModel"/>.
    /// </summary>
    public class OperationsHistoryListModelTests
    {
        /// <summary>
        /// Test the model should start empty.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartEmpty()
        {
            // Arrange & Act
            OperationsHistoryListModel model = new();

            // Assert
            model.EntityID.Should().BeNull();
            model.EntityName.Should().BeNull();
            model.ID.Should().Be(0);
            model.NewValues.Should().BeNull();
            model.OldValues.Should().BeNull();
            model.OperationType.Should().BeNull();
            model.TableName.Should().BeNull();
        }

        /// <summary>
        /// Test the model should expose every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeEveryValue()
        {
            // Arrange & Act
            OperationsHistoryListModel model = new()
            {
                EntityID = 3,
                EntityName = "EntityFake",
                ID = 7,
                NewValues = "{\"Name\":\"new\"}",
                OldValues = "{\"Name\":\"old\"}",
                OperationType = "Modified",
                TableName = "Entities",
            };

            // Assert
            model.EntityID.Should().Be(3);
            model.EntityName.Should().Be("EntityFake");
            model.ID.Should().Be(7);
            model.NewValues.Should().Be("{\"Name\":\"new\"}");
            model.OldValues.Should().Be("{\"Name\":\"old\"}");
            model.OperationType.Should().Be("Modified");
            model.TableName.Should().Be("Entities");
        }
    }
}
