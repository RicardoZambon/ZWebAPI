using ZWebAPI.Models;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.Models
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Models.BatchUpdateModel{TEntity, TKey}"/>.
    /// </summary>
    public class BatchUpdateModelTests
    {
        /// <summary>
        /// Test the model should start with both collections unset.
        /// </summary>
        [Fact]
        public void Defaults_Pass_StartWithBothCollectionsNull()
        {
            // Arrange & Act
            BatchUpdateModel<EntityFake, long> model = new();

            // Assert
            model.EntitiesToDelete.Should().BeNull();
            model.EntitiesToInsertOrUpdate.Should().BeNull();
        }

        /// <summary>
        /// Test the model should expose every value it received.
        /// </summary>
        [Fact]
        public void Properties_Pass_ExposeTheBatchContract()
        {
            // Arrange
            long[] toDelete = [1L, 2L];
            EntityFake[] toUpsert = [new(), new(), new()];

            // Act
            BatchUpdateModel<EntityFake, long> model = new()
            {
                EntitiesToDelete = toDelete,
                EntitiesToInsertOrUpdate = toUpsert,
            };

            // Assert
            model.EntitiesToDelete.Should().BeSameAs(toDelete);
            model.EntitiesToInsertOrUpdate.Should().BeSameAs(toUpsert);
        }

        /// <summary>
        /// Test the model should accept empty batches, which is a no-op update rather than an error.
        /// </summary>
        [Fact]
        public void Properties_Pass_AcceptEmptyBatches()
        {
            // Arrange & Act
            BatchUpdateModel<EntityFake, long> model = new()
            {
                EntitiesToDelete = [],
                EntitiesToInsertOrUpdate = [],
            };

            // Assert
            model.EntitiesToDelete.Should().BeEmpty();
            model.EntitiesToInsertOrUpdate.Should().BeEmpty();
        }

        /// <summary>
        /// Test the model should allow either side of the batch to be cleared independently.
        /// </summary>
        [Fact]
        public void Properties_Pass_AllowEitherSideToBeClearedIndependently()
        {
            // Arrange
            BatchUpdateModel<EntityFake, long> model = new()
            {
                EntitiesToDelete = [1L],
                EntitiesToInsertOrUpdate = [new EntityFake()],
            };

            // Act
            model.EntitiesToDelete = null;

            // Assert
            model.EntitiesToDelete.Should().BeNull();
            model.EntitiesToInsertOrUpdate.Should().HaveCount(1);
        }
    }
}
