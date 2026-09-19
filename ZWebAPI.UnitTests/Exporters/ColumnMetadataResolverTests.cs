using ZWebAPI.Exporters;
using ZWebAPI.UnitTests.Fakes.ExportersFake;

namespace ZWebAPI.UnitTests.Exporters
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Exporters.ColumnMetadataResolver"/>.
    /// </summary>
    public class ColumnMetadataResolverTests
    {
        /// <summary>
        /// Test the Resolve should throw when the row type is null.
        /// </summary>
        [Fact]
        public void Resolve_Fail_ThrowsWhenRowTypeIsNull()
        {
            // Arrange
            Type? rowType = null;

            // Act
            Action act = () => ColumnMetadataResolver.Resolve(rowType!);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithParameterName("rowType");
        }

        /// <summary>
        /// Test the Resolve should use the property name when the attribute is absent.
        /// </summary>
        [Fact]
        public void Resolve_Pass_UsesPropertyNameWhenAttributeIsAbsent()
        {
            // Arrange
            Type rowType = typeof(ExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().Contain(x => x.Header == nameof(ExportRowFake.ID));
        }

        /// <summary>
        /// Test the Resolve should use the header declared on the attribute.
        /// </summary>
        [Fact]
        public void Resolve_Pass_UsesCustomHeaderFromAttribute()
        {
            // Arrange
            Type rowType = typeof(ExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().Contain(x => x.Header == "Full name");
            columns.Should().NotContain(x => x.Header == nameof(ExportRowFake.Name));
        }

        /// <summary>
        /// Test the Resolve should fall back to the property name when the attribute header is blank.
        /// </summary>
        [Fact]
        public void Resolve_Pass_FallsBackToPropertyNameWhenHeaderIsBlank()
        {
            // Arrange
            Type rowType = typeof(BlankHeaderExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().ContainSingle();
            columns[0].Header.Should().Be(nameof(BlankHeaderExportRowFake.Blank));
        }

        /// <summary>
        /// Test the Resolve should skip properties flagged as ignored.
        /// </summary>
        [Fact]
        public void Resolve_Pass_SkipsIgnoredProperties()
        {
            // Arrange
            Type rowType = typeof(ExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().NotContain(x => x.Header == nameof(ExportRowFake.Secret));
            columns.Should().HaveCount(7);
        }

        /// <summary>
        /// Test the Resolve should skip write-only properties.
        /// </summary>
        [Fact]
        public void Resolve_Pass_SkipsWriteOnlyProperties()
        {
            // Arrange
            Type rowType = typeof(WriteOnlyExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().ContainSingle();
            columns[0].Header.Should().Be(nameof(WriteOnlyExportRowFake.Readable));
        }

        /// <summary>
        /// Test the Resolve should order explicitly ordered columns first and keep declaration order otherwise.
        /// </summary>
        [Fact]
        public void Resolve_Pass_OrdersByExplicitOrderThenDeclaration()
        {
            // Arrange
            Type rowType = typeof(OrderedExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Select(x => x.Header).Should().ContainInOrder(
                nameof(OrderedExportRowFake.First),
                nameof(OrderedExportRowFake.Second),
                nameof(OrderedExportRowFake.Third),
                nameof(OrderedExportRowFake.Fourth));
        }

        /// <summary>
        /// Test the Resolve should return an empty list for a type without exportable properties.
        /// </summary>
        [Fact]
        public void Resolve_Pass_ReturnsEmptyForTypeWithoutProperties()
        {
            // Arrange
            Type rowType = typeof(EmptyExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Should().BeEmpty();
        }

        /// <summary>
        /// Test the Resolve should default the column type to Text and honor the declared type otherwise.
        /// </summary>
        [Fact]
        public void Resolve_Pass_UsesDeclaredColumnTypeOrDefaultsToText()
        {
            // Arrange
            Type rowType = typeof(ExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            columns.Single(x => x.Header == nameof(ExportRowFake.ID)).Type.Should().Be(ExportColumnType.Text);
            columns.Single(x => x.Header == nameof(ExportRowFake.Amount)).Type.Should().Be(ExportColumnType.Number);
            columns.Single(x => x.Header == nameof(ExportRowFake.Price)).Type.Should().Be(ExportColumnType.Currency);
            columns.Single(x => x.Header == nameof(ExportRowFake.CreatedOn)).Type.Should().Be(ExportColumnType.Date);
            columns.Single(x => x.Header == nameof(ExportRowFake.ChangedOn)).Type.Should().Be(ExportColumnType.DateTime);
            columns.Single(x => x.Header == nameof(ExportRowFake.IsActive)).Type.Should().Be(ExportColumnType.Boolean);
        }

        /// <summary>
        /// Test the Resolve should build selectors that read the value from the row instance.
        /// </summary>
        [Fact]
        public void Resolve_Pass_BuildsSelectorReadingTheRowValue()
        {
            // Arrange
            ExportRowFake row = new() { ID = 7, Name = "Row name" };

            // Act
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(ExportRowFake));

            // Assert
            columns.Single(x => x.Header == nameof(ExportRowFake.ID)).ValueSelector(row).Should().Be(7L);
            columns.Single(x => x.Header == "Full name").ValueSelector(row).Should().Be("Row name");
        }

        /// <summary>
        /// Test the Resolve should serve the cached metadata on subsequent calls for the same type.
        /// </summary>
        [Fact]
        public void Resolve_Pass_ReturnsCachedResultForTheSameType()
        {
            // Arrange
            Type rowType = typeof(ExportRowFake);

            // Act
            IReadOnlyList<ExportColumn> first = ColumnMetadataResolver.Resolve(rowType);
            IReadOnlyList<ExportColumn> second = ColumnMetadataResolver.Resolve(rowType);

            // Assert
            second.Should().BeSameAs(first);
        }
    }
}
