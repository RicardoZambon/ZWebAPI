using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ListResult{T}"/>.
    /// </summary>
    public class ListResultTests
    {
        /// <summary>
        /// Test the constructor should expose the items and the total row count.
        /// </summary>
        [Fact]
        public void Constructor_Pass_ExposesItemsAndTotalRows()
        {
            // Arrange
            List<string> items = ["a", "b"];

            // Act
            ListResult<string> result = new(items, 42);

            // Assert
            result.Items.Should().BeSameAs(items);
            result.TotalRows.Should().Be(42);
        }

        /// <summary>
        /// Test the From should keep the same instance when the items are already a list.
        /// </summary>
        [Fact]
        public void From_Pass_KeepsTheInstanceWhenItemsAreAlreadyAList()
        {
            // Arrange
            List<string> items = ["a", "b"];

            // Act
            ListResult<string> result = ListResult<string>.From(items, 2);

            // Assert
            result.Items.Should().BeSameAs(items);
            result.TotalRows.Should().Be(2);
        }

        /// <summary>
        /// Test the From should materialize a deferred sequence.
        /// </summary>
        [Fact]
        public void From_Pass_MaterializesDeferredSequences()
        {
            // Arrange
            int evaluations = 0;
            IEnumerable<string> items = new[] { "a", "b" }.Select(x =>
            {
                evaluations++;
                return x;
            });

            // Act
            ListResult<string> result = ListResult<string>.From(items, 2);

            // Assert
            evaluations.Should().Be(2);
            result.Items.Should().BeOfType<List<string>>();
            result.Items.Should().ContainInOrder("a", "b");
        }

        /// <summary>
        /// Test the From should accept an empty sequence.
        /// </summary>
        [Fact]
        public void From_Pass_AcceptsAnEmptySequence()
        {
            // Arrange
            IEnumerable<string> items = Enumerable.Empty<string>();

            // Act
            ListResult<string> result = ListResult<string>.From(items, 0);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalRows.Should().Be(0);
        }
    }
}
