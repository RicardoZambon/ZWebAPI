using ZWebAPI.Models;

namespace ZWebAPI.UnitTests.Models
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.RelationshipUpdateModel{TKey}"/>.
    /// </summary>
    public class RelationshipUpdateModelTests
    {
        /// <summary>
        /// Test the model should start with both sides empty rather than null, so callers can
        /// enumerate without a null check.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartWithBothSidesEmpty()
        {
            // Arrange & Act
            RelationshipUpdateModel<long> model = new();

            // Assert
            model.IDsToAdd.Should().NotBeNull();
            model.IDsToAdd.Should().BeEmpty();
            model.IDsToRemove.Should().NotBeNull();
            model.IDsToRemove.Should().BeEmpty();
        }

        /// <summary>
        /// Test the model should expose every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeTheRelationshipContract()
        {
            // Arrange
            long[] toAdd = [1L, 2L];
            long[] toRemove = [3L];

            // Act
            RelationshipUpdateModel<long> model = new()
            {
                IDsToAdd = toAdd,
                IDsToRemove = toRemove,
            };

            // Assert
            model.IDsToAdd.Should().Equal(1L, 2L);
            model.IDsToRemove.Should().Equal(3L);
        }

        /// <summary>
        /// Test the model should support a key type other than long.
        /// </summary>
        [Fact]
        public void Properties_Pass_SupportAnAlternateKeyType()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            // Act
            RelationshipUpdateModel<Guid> model = new()
            {
                IDsToAdd = [id],
            };

            // Assert
            model.IDsToAdd.Should().Equal(id);
            model.IDsToRemove.Should().BeEmpty();
        }

        /// <summary>
        /// Test the model should allow the same identifier on both sides, which the caller resolves
        /// rather than the model rejecting it.
        /// </summary>
        [Fact]
        public void Properties_Pass_AllowTheSameIdentifierOnBothSides()
        {
            // Arrange & Act
            RelationshipUpdateModel<long> model = new()
            {
                IDsToAdd = [7L],
                IDsToRemove = [7L],
            };

            // Assert
            model.IDsToAdd.Should().Equal(7L);
            model.IDsToRemove.Should().Equal(7L);
        }
    }
}
