using System.Text.Json;
using ZWebAPI.Enums;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.Interfaces;
using ZWebAPI.Models;
using ZWebAPI.Models.Catalog;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.ExtensionMethods
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.ExtensionMethods.QueryableExtensions"/>.
    /// </summary>
    public class QueryableExtensionsTests
    {
        /// <summary>
        /// Test the GetRange should skip the rows before the start row.
        /// </summary>
        [Fact]
        public void GetRange_Pass_SkipsTheRowsBeforeTheStartRow()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { StartRow = 1 };

            // Act
            IQueryable<EntityFake> result = context.Entities.OrderBy(x => x.ID).GetRange(parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L, 3L);
        }

        /// <summary>
        /// Test the GetRange should take at most the end row count.
        /// </summary>
        [Fact]
        public void GetRange_Pass_TakesAtMostTheEndRowCount()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { EndRow = 2 };

            // Act
            IQueryable<EntityFake> result = context.Entities.OrderBy(x => x.ID).GetRange(parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(1L, 2L);
        }

        /// <summary>
        /// Test the GetRange should combine the start row and the end row.
        /// </summary>
        [Fact]
        public void GetRange_Pass_CombinesTheStartRowAndTheEndRow()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 1 };

            // Act
            IQueryable<EntityFake> result = context.Entities.OrderBy(x => x.ID).GetRange(parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the GetRange should leave the query untouched for non positive boundaries.
        /// </summary>
        /// <param name="startRow">The start row.</param>
        /// <param name="endRow">The end row.</param>
        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        public void GetRange_Pass_LeavesTheQueryUntouchedForNonPositiveBoundaries(int startRow, int endRow)
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { StartRow = startRow, EndRow = endRow };
            IQueryable<EntityFake> query = context.Entities.OrderBy(x => x.ID);

            // Act
            IQueryable<EntityFake> result = query.GetRange(parameters);

            // Assert
            result.Should().BeSameAs(query);
            result.Select(x => x.ID).Should().Equal(1L, 2L, 3L);
        }

        /// <summary>
        /// Test the GetRange should leave the query untouched when the parameters are null.
        /// </summary>
        [Fact]
        public void GetRange_Pass_LeavesTheQueryUntouchedWhenTheParametersAreNull()
        {
            // Arrange
            using DbContextFake context = Seed();
            IQueryable<EntityFake> query = context.Entities.OrderBy(x => x.ID);

            // Act
            IQueryable<EntityFake> result = query.GetRange(null!);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilter should filter on the property name inferred from the selector.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_FiltersOnTheInferredPropertyName()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("Age", "20"));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => x.Age, FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the TryFilter should compose the property name from a nested member selector.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_ComposesTheNameFromANestedMemberSelector()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("ChildName", "\"Delta\""));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => x.Child!.Name, FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the TryFilter should unwrap a converting selector before reading the member.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_UnwrapsAConvertingSelector()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("Age", "20"));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => (long)x.Age, FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the TryFilter should leave the query untouched when the selector is not a member access.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_LeavesTheQueryUntouchedForANonMemberSelector()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("Age", "20"));
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilter(parameters, x => x.Age + 1, FilterTypes.Equals);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilter should leave the query untouched when no filter was sent for the property.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_LeavesTheQueryUntouchedWhenTheFilterIsAbsent()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("Name", "\"Alpha\""));
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilter(parameters, x => x.Age, FilterTypes.Equals);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilter should read a filter value into a nullable property instead of throwing.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_ReadsAFilterValueIntoANullableProperty()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("ParentID", "\"5\""));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => x.ParentID, FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L, 3L);
        }

        /// <summary>
        /// Test the TryFilter should match the empty values of a nullable property for an explicit null filter.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_MatchesEmptyValuesForAnExplicitNullFilter()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("ParentID", "null"));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => x.ParentID, FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(1L);
        }

        /// <summary>
        /// Test the TryFilter should filter on the explicitly named parameter.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_FiltersOnTheExplicitlyNamedParameter()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("MinimumAge", "30"));

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilter(parameters, x => x.Age, "MinimumAge", FilterTypes.Equals);

            // Assert
            result.Select(x => x.ID).Should().Equal(3L);
        }

        /// <summary>
        /// Test the TryFilter should leave the query untouched when the explicitly named filter is absent.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_LeavesTheQueryUntouchedWhenTheNamedFilterIsAbsent()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("Age", "30"));
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilter(parameters, x => x.Age, "MinimumAge", FilterTypes.Equals);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilter should leave the query untouched when the named filter selector is not a member access.
        /// </summary>
        [Fact]
        public void TryFilter_Pass_LeavesTheQueryUntouchedForANamedNonMemberSelector()
        {
            // Arrange
            using DbContextFake context = Seed();
            ISummaryParameters parameters = Build(("MinimumAge", "30"));
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilter(parameters, x => x.Age + 1, "MinimumAge", FilterTypes.Equals);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilterWithValue should leave the query untouched when the selector is not a member access.
        /// </summary>
        [Fact]
        public void TryFilterWithValue_Pass_LeavesTheQueryUntouchedForANonMemberSelector()
        {
            // Arrange
            using DbContextFake context = Seed();
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilterWithValue(x => x.Age + 1, 20, FilterTypes.Equals);

            // Assert
            result.Should().BeSameAs(query);
        }

        /// <summary>
        /// Test the TryFilterWithValue should wrap the value in wildcards for a Like filter.
        /// </summary>
        [Fact]
        public void TryFilterWithValue_Pass_WrapsTheValueInWildcardsForLike()
        {
            // Arrange
            using DbContextFake context = Seed();

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilterWithValue(x => x.Name, "et", FilterTypes.Like);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the TryFilterWithValue should apply the comparison matching the filter type.
        /// </summary>
        /// <param name="filterType">The filter type under test.</param>
        /// <param name="expected">The identifiers expected to survive the filter.</param>
        [Theory]
        [InlineData(FilterTypes.Equals, new long[] { 2 })]
        [InlineData(FilterTypes.LessThan, new long[] { 1 })]
        [InlineData(FilterTypes.LessThanOrEqual, new long[] { 1, 2 })]
        [InlineData(FilterTypes.GreaterThan, new long[] { 3 })]
        [InlineData(FilterTypes.GreatherThanOrEqual, new long[] { 2, 3 })]
        public void TryFilterWithValue_Pass_AppliesTheComparisonMatchingTheFilterType(FilterTypes filterType, long[] expected)
        {
            // Arrange
            using DbContextFake context = Seed();

            // Act
            IQueryable<EntityFake> result = context.Entities.TryFilterWithValue(x => x.Age, 20, filterType);

            // Assert
            result.OrderBy(x => x.ID).Select(x => x.ID).Should().Equal(expected);
        }

        /// <summary>
        /// Test the TryFilterWithValue should leave the query untouched for an unknown filter type.
        /// </summary>
        [Fact]
        public void TryFilterWithValue_Pass_LeavesTheQueryUntouchedForAnUnknownFilterType()
        {
            // Arrange
            using DbContextFake context = Seed();
            IQueryable<EntityFake> query = context.Entities;

            // Act
            IQueryable<EntityFake> result = query.TryFilterWithValue(x => x.Age, 20, (FilterTypes)99);

            // Assert
            result.Should().BeSameAs(query);
            result.Should().HaveCount(3);
        }

        /// <summary>
        /// Test the GetCatalog should project every row ordered by its display when no criteria is sent.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_ProjectsEveryRowOrderedByDisplay()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel();

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.ShouldUseCriteria.Should().BeFalse();
            result.Entries.Select(x => x.Display).Should().Equal("Alpha", "Beta", "Charlie");
            result.Entries.Select(x => x.Value).Should().Equal(1L, 2L, 3L);
        }

        /// <summary>
        /// Test the GetCatalog should narrow the entries with a case insensitive criteria.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_NarrowsTheEntriesWithTheCriteria()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { Criteria = "ET" };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.Entries.Select(x => x.Display).Should().Equal("Beta");
        }

        /// <summary>
        /// Test the GetCatalog should ignore a criteria made only of whitespace.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_IgnoresAWhitespaceCriteria()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { Criteria = "   " };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.Entries.Should().HaveCount(3);
        }

        /// <summary>
        /// Test the GetCatalog should apply the custom filter instead of the default one.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_AppliesTheCustomFilter()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { Criteria = "anything" };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name, x => x.Value > 2);

            // Assert
            result.Entries.Select(x => x.Display).Should().Equal("Charlie");
        }

        /// <summary>
        /// Test the GetCatalog should ask for a criteria when too many rows match and none was sent.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_AsksForACriteriaWhenTooManyRowsMatch()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { MaxResults = 2 };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.ShouldUseCriteria.Should().BeTrue();
            result.Entries.Should().BeEmpty();
        }

        /// <summary>
        /// Test the GetCatalog should cap the entries when too many rows match a criteria.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_CapsTheEntriesWhenTooManyRowsMatchACriteria()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { Criteria = "a", MaxResults = 1 };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.ShouldUseCriteria.Should().BeFalse();
            result.Entries.Should().ContainSingle();
        }

        /// <summary>
        /// Test the GetCatalog should not cap the entries when the row count fits the maximum.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_DoesNotCapWhenTheRowCountFitsTheMaximum()
        {
            // Arrange
            using DbContextFake context = Seed();
            ICatalogParameters parameters = new CatalogParametersModel { MaxResults = 3 };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.ShouldUseCriteria.Should().BeFalse();
            result.Entries.Should().HaveCount(3);
        }

        /// <summary>
        /// Test the GetCatalog should return an empty catalog for an empty source.
        /// </summary>
        [Fact]
        public void GetCatalog_Pass_ReturnsAnEmptyCatalogForAnEmptySource()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            ICatalogParameters parameters = new CatalogParametersModel { MaxResults = 10 };

            // Act
            CatalogResultModel<long> result = context.Entities.GetCatalog(parameters, x => x.ID, x => x.Name);

            // Assert
            result.ShouldUseCriteria.Should().BeFalse();
            result.Entries.Should().BeEmpty();
        }

        private static SummaryParametersModel Build(params (string Key, string Json)[] filters)
        {
            SummaryParametersModel parameters = new();

            foreach ((string key, string json) in filters)
            {
                parameters.Filters![key] = JsonDocument.Parse(json).RootElement;
            }

            return parameters;
        }

        private static DbContextFake Seed()
        {
            DbContextFake context = DbContextFakeFactory.Create();

            context.Entities.AddRange(
                new EntityFake
                {
                    ID = 1,
                    Name = "Alpha",
                    Age = 10,
                    Price = 10.5m,
                    ParentID = null,
                    Status = StatusFake.None,
                    IsActive = true,
                    CreatedOn = new DateTime(2024, 1, 1),
                    Child = new ChildFake { ID = 11, Name = "Gamma" },
                },
                new EntityFake
                {
                    ID = 2,
                    Name = "Beta",
                    Age = 20,
                    Price = 20.5m,
                    ParentID = 5,
                    Status = StatusFake.Active,
                    IsActive = false,
                    CreatedOn = new DateTime(2024, 2, 1),
                    Child = new ChildFake { ID = 12, Name = "Delta" },
                },
                new EntityFake
                {
                    ID = 3,
                    Name = "Charlie",
                    Age = 30,
                    Price = 30.5m,
                    ParentID = 5,
                    Status = StatusFake.Inactive,
                    IsActive = true,
                    CreatedOn = new DateTime(2024, 3, 1),
                    Child = new ChildFake { ID = 13, Name = "Epsilon" },
                });

            context.SaveChanges();
            context.ChangeTracker.Clear();

            return context;
        }
    }
}
