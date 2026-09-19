using AutoMapper;
using ZWebAPI.Exporters;
using ZWebAPI.Exporters.Exceptions;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.Interfaces;
using ZWebAPI.Models;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.ExtensionMethods
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.ExtensionMethods.QueryableListExtensions"/>.
    /// </summary>
    public class QueryableListExtensionsTests
    {
        /// <summary>
        /// Test the projecting ToExportResultAsync should hand the projected rows to the exporter.
        /// </summary>
        [Fact]
        public async Task ToExportResultAsyncProjecting_Pass_HandsTheProjectedRowsToTheExporter()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel();
            IConfigurationProvider mapperConfig = MapperFakeFactory.CreateConfiguration();
            ExportResult expected = new([1], "text/csv", "report.csv");
            IListExporter listExporter = Substitute.For<IListExporter>();
            List<EntityListModelFake>? captured = null;

            listExporter
                .Export(Arg.Any<IEnumerable<EntityListModelFake>>(), Arg.Any<ExportFormat>(), Arg.Any<string>())
                .Returns(x =>
                {
                    captured = [.. x.ArgAt<IEnumerable<EntityListModelFake>>(0)];
                    return expected;
                });

            // Act
            ExportResult result = await context.Entities
                .ToExportResultAsync<EntityFake, EntityListModelFake>(parameters, mapperConfig, listExporter, ExportFormat.Csv, "report");

            // Assert
            result.Should().BeSameAs(expected);
            captured.Should().NotBeNull();
            captured!.Select(x => x.Name).Should().BeEquivalentTo(["Alpha", "Beta", "Charlie"]);
            listExporter.Received(1).Export(Arg.Any<IEnumerable<EntityListModelFake>>(), ExportFormat.Csv, "report");
        }

        /// <summary>
        /// Test the projecting ToExportResultAsync should drop the pagination requested by the caller.
        /// </summary>
        [Fact]
        public async Task ToExportResultAsyncProjecting_Pass_DropsTheRequestedPagination()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 1 };
            IConfigurationProvider mapperConfig = MapperFakeFactory.CreateConfiguration();
            IListExporter listExporter = Substitute.For<IListExporter>();
            List<EntityListModelFake>? captured = null;

            listExporter
                .Export(Arg.Any<IEnumerable<EntityListModelFake>>(), Arg.Any<ExportFormat>(), Arg.Any<string>())
                .Returns(x =>
                {
                    captured = [.. x.ArgAt<IEnumerable<EntityListModelFake>>(0)];
                    return new ExportResult([], "text/csv", "report.csv");
                });

            // Act
            await context.Entities
                .ToExportResultAsync<EntityFake, EntityListModelFake>(parameters, mapperConfig, listExporter, ExportFormat.Csv, "report");

            // Assert
            parameters.StartRow.Should().Be(0);
            parameters.EndRow.Should().Be(int.MaxValue);
            captured.Should().HaveCount(3);
        }

        /// <summary>
        /// Test the projecting ToExportResultAsync should reject a row count above the export limit.
        /// </summary>
        [Fact]
        public async Task ToExportResultAsyncProjecting_Fail_ThrowsAboveTheExportRowLimit()
        {
            // Arrange
            IQueryable<EntityFake> query = OversizedQuery();
            IListParameters parameters = new ListParametersModel();
            IConfigurationProvider mapperConfig = MapperFakeFactory.CreateConfiguration();
            IListExporter listExporter = Substitute.For<IListExporter>();

            // Act
            Func<Task> act = () => query
                .ToExportResultAsync<EntityFake, EntityListModelFake>(parameters, mapperConfig, listExporter, ExportFormat.Csv, "report");

            // Assert
            (await act.Should().ThrowAsync<ExportRowLimitExceededException>())
                .Which.MaxRows.Should().Be(ExportConstants.MaxExportRows);

            listExporter.DidNotReceiveWithAnyArgs().Export(Arg.Any<IEnumerable<EntityListModelFake>>(), default, default!);
        }

        /// <summary>
        /// Test the projecting ToExportResultAsync should reject the missing arguments.
        /// </summary>
        /// <param name="missing">The argument left out.</param>
        [Theory]
        [InlineData("query")]
        [InlineData("parameters")]
        [InlineData("mapperConfig")]
        [InlineData("listExporter")]
        public async Task ToExportResultAsyncProjecting_Fail_ThrowsForMissingArguments(string missing)
        {
            // Arrange
            IQueryable<EntityFake>? query = missing == "query" ? null : Array.Empty<EntityFake>().AsQueryable();
            IListParameters? parameters = missing == "parameters" ? null : new ListParametersModel();
            IConfigurationProvider? mapperConfig = missing == "mapperConfig" ? null : MapperFakeFactory.CreateConfiguration();
            IListExporter? listExporter = missing == "listExporter" ? null : Substitute.For<IListExporter>();

            // Act
            Func<Task> act = () => query!
                .ToExportResultAsync<EntityFake, EntityListModelFake>(parameters!, mapperConfig!, listExporter!, ExportFormat.Csv, "report");

            // Assert
            (await act.Should().ThrowAsync<ArgumentNullException>())
                .WithParameterName(missing);
        }

        /// <summary>
        /// Test the projected ToExportResultAsync should hand the already projected rows to the exporter.
        /// </summary>
        [Fact]
        public async Task ToExportResultAsyncProjected_Pass_HandsTheRowsToTheExporter()
        {
            // Arrange
            IQueryable<EntityListModelFake> query = new List<EntityListModelFake>
            {
                new() { ID = 1, Name = "Alpha" },
                new() { ID = 2, Name = "Beta" },
            }.AsQueryable();
            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 1 };
            ExportResult expected = new([1], "text/csv", "report.csv");
            IListExporter listExporter = Substitute.For<IListExporter>();
            List<EntityListModelFake>? captured = null;

            listExporter
                .Export(Arg.Any<IEnumerable<EntityListModelFake>>(), Arg.Any<ExportFormat>(), Arg.Any<string>())
                .Returns(x =>
                {
                    captured = [.. x.ArgAt<IEnumerable<EntityListModelFake>>(0)];
                    return expected;
                });

            // Act
            ExportResult result = await query.ToExportResultAsync(parameters, listExporter, ExportFormat.Xlsx, "report");

            // Assert
            result.Should().BeSameAs(expected);
            parameters.StartRow.Should().Be(0);
            parameters.EndRow.Should().Be(int.MaxValue);
            captured!.Select(x => x.ID).Should().Equal(1L, 2L);
        }

        /// <summary>
        /// Test the projected ToExportResultAsync should reject a row count above the export limit.
        /// </summary>
        [Fact]
        public async Task ToExportResultAsyncProjected_Fail_ThrowsAboveTheExportRowLimit()
        {
            // Arrange
            IQueryable<EntityListModelFake> query = Enumerable
                .Range(1, ExportConstants.MaxExportRows + 1)
                .Select(x => new EntityListModelFake { ID = x })
                .AsQueryable();
            IListParameters parameters = new ListParametersModel();
            IListExporter listExporter = Substitute.For<IListExporter>();

            // Act
            Func<Task> act = () => query.ToExportResultAsync(parameters, listExporter, ExportFormat.Csv, "report");

            // Assert
            (await act.Should().ThrowAsync<ExportRowLimitExceededException>())
                .WithMessage($"The export result exceeds the configured limit of {ExportConstants.MaxExportRows} rows.");
        }

        /// <summary>
        /// Test the projected ToExportResultAsync should reject the missing arguments.
        /// </summary>
        /// <param name="missing">The argument left out.</param>
        [Theory]
        [InlineData("query")]
        [InlineData("parameters")]
        [InlineData("listExporter")]
        public async Task ToExportResultAsyncProjected_Fail_ThrowsForMissingArguments(string missing)
        {
            // Arrange
            IQueryable<EntityListModelFake>? query = missing == "query" ? null : Array.Empty<EntityListModelFake>().AsQueryable();
            IListParameters? parameters = missing == "parameters" ? null : new ListParametersModel();
            IListExporter? listExporter = missing == "listExporter" ? null : Substitute.For<IListExporter>();

            // Act
            Func<Task> act = () => query!.ToExportResultAsync(parameters!, listExporter!, ExportFormat.Csv, "report");

            // Assert
            (await act.Should().ThrowAsync<ArgumentNullException>())
                .WithParameterName(missing);
        }

        /// <summary>
        /// Test the projecting ToListResultAsync should paginate the items and keep the full row count.
        /// </summary>
        [Fact]
        public async Task ToListResultAsyncProjecting_Pass_PaginatesItemsAndKeepsTheFullRowCount()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 1 };
            IConfigurationProvider mapperConfig = MapperFakeFactory.CreateConfiguration();

            // Act
            ListResult<EntityListModelFake> result = await context.Entities
                .OrderBy(x => x.ID)
                .ToListResultAsync<EntityFake, EntityListModelFake>(parameters, mapperConfig);

            // Assert
            result.TotalRows.Should().Be(3);
            result.Items.Select(x => x.Name).Should().Equal("Beta");
        }

        /// <summary>
        /// Test the projecting ToListResultAsync should return an empty envelope for an empty source.
        /// </summary>
        [Fact]
        public async Task ToListResultAsyncProjecting_Pass_ReturnsAnEmptyEnvelopeForAnEmptySource()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            IListParameters parameters = new ListParametersModel();
            IConfigurationProvider mapperConfig = MapperFakeFactory.CreateConfiguration();

            // Act
            ListResult<EntityListModelFake> result = await context.Entities
                .ToListResultAsync<EntityFake, EntityListModelFake>(parameters, mapperConfig);

            // Assert
            result.TotalRows.Should().Be(0);
            result.Items.Should().BeEmpty();
        }

        /// <summary>
        /// Test the projecting ToListResultAsync should reject the missing arguments.
        /// </summary>
        /// <param name="missing">The argument left out.</param>
        [Theory]
        [InlineData("query")]
        [InlineData("parameters")]
        [InlineData("mapperConfig")]
        public async Task ToListResultAsyncProjecting_Fail_ThrowsForMissingArguments(string missing)
        {
            // Arrange
            IQueryable<EntityFake>? query = missing == "query" ? null : Array.Empty<EntityFake>().AsQueryable();
            IListParameters? parameters = missing == "parameters" ? null : new ListParametersModel();
            IConfigurationProvider? mapperConfig = missing == "mapperConfig" ? null : MapperFakeFactory.CreateConfiguration();

            // Act
            Func<Task> act = () => query!.ToListResultAsync<EntityFake, EntityListModelFake>(parameters!, mapperConfig!);

            // Assert
            (await act.Should().ThrowAsync<ArgumentNullException>())
                .WithParameterName(missing);
        }

        /// <summary>
        /// Test the projected ToListResultAsync should paginate a synchronous query.
        /// </summary>
        [Fact]
        public async Task ToListResultAsyncProjected_Pass_PaginatesASynchronousQuery()
        {
            // Arrange
            IQueryable<EntityListModelFake> query = new List<EntityListModelFake>
            {
                new() { ID = 1, Name = "Alpha" },
                new() { ID = 2, Name = "Beta" },
                new() { ID = 3, Name = "Charlie" },
            }.AsQueryable();
            IListParameters parameters = new ListParametersModel { StartRow = 2, EndRow = 5 };

            // Act
            ListResult<EntityListModelFake> result = await query.ToListResultAsync(parameters);

            // Assert
            result.TotalRows.Should().Be(3);
            result.Items.Select(x => x.ID).Should().Equal(3L);
        }

        /// <summary>
        /// Test the projected ToListResultAsync should paginate an asynchronous query.
        /// </summary>
        [Fact]
        public async Task ToListResultAsyncProjected_Pass_PaginatesAnAsynchronousQuery()
        {
            // Arrange
            using DbContextFake context = Seed();
            IListParameters parameters = new ListParametersModel { EndRow = 2 };

            // Act
            ListResult<EntityFake> result = await context.Entities.OrderBy(x => x.ID).ToListResultAsync(parameters);

            // Assert
            result.TotalRows.Should().Be(3);
            result.Items.Select(x => x.ID).Should().Equal(1L, 2L);
        }

        /// <summary>
        /// Test the projected ToListResultAsync should reject the missing arguments.
        /// </summary>
        /// <param name="missing">The argument left out.</param>
        [Theory]
        [InlineData("query")]
        [InlineData("parameters")]
        public async Task ToListResultAsyncProjected_Fail_ThrowsForMissingArguments(string missing)
        {
            // Arrange
            IQueryable<EntityListModelFake>? query = missing == "query" ? null : Array.Empty<EntityListModelFake>().AsQueryable();
            IListParameters? parameters = missing == "parameters" ? null : new ListParametersModel();

            // Act
            Func<Task> act = () => query!.ToListResultAsync(parameters!);

            // Assert
            (await act.Should().ThrowAsync<ArgumentNullException>())
                .WithParameterName(missing);
        }

        private static IQueryable<EntityFake> OversizedQuery()
        {
            return Enumerable
                .Range(1, ExportConstants.MaxExportRows + 1)
                .Select(x => new EntityFake { ID = x })
                .AsQueryable();
        }

        private static DbContextFake Seed()
        {
            DbContextFake context = DbContextFakeFactory.Create();

            context.Entities.AddRange(
                new EntityFake { ID = 1, Name = "Alpha" },
                new EntityFake { ID = 2, Name = "Beta" },
                new EntityFake { ID = 3, Name = "Charlie" });

            context.SaveChanges();
            context.ChangeTracker.Clear();

            return context;
        }
    }
}
