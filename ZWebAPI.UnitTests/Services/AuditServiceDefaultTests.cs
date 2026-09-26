using AutoMapper;
using System.Text.Json;
using ZDatabase.Exceptions;
using ZDatabase.Repositories.Audit.Interfaces;
using ZSecurity.Services;
using ZWebAPI.Interfaces;
using ZWebAPI.Models;
using ZWebAPI.Models.Audit;
using ZWebAPI.Models.Audit.OperationHistory;
using ZWebAPI.Models.Audit.ServiceHistory;
using ZWebAPI.Services;
using ZWebAPI.Services.Interfaces;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;
using ZWebAPI.UnitTests.Fakes.ServicesFake;

namespace ZWebAPI.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Services.AuditServiceDefault{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/>.
    /// </summary>
    public class AuditServiceDefaultTests
    {
        /// <summary>
        /// Test the BeginNewServiceHistoryAsync should name the service history after the calling
        /// service contract and method, and register it.
        /// </summary>
        [Fact]
        public async Task BeginNewServiceHistoryAsync_Pass_RegistersTheServiceHistoryNamedAfterTheCaller()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            AuditedServiceFake service = new(harness.Service);

            // Act
            await service.BeginAsync();

            // Assert
            await harness.SecurityHandler.Received(1).ValidateUserHasPermissionAsync();
            harness.AddedServicesHistory.Should().ContainSingle();
            harness.AddedServicesHistory[0].Name.Should().Be($"{nameof(IAuditedServiceFake)}\\{nameof(IAuditedServiceFake.BeginAsync)}");
        }

        /// <summary>
        /// Test the BeginNewServiceHistoryAsync should refuse a caller that is not an action method.
        /// </summary>
        [Fact]
        public async Task BeginNewServiceHistoryAsync_Fail_ThrowsWhenTheCallerIsNotAnActionMethod()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            UnauditedServiceFake service = new(harness.Service);

            // Act
            Func<Task> act = service.BeginAsync;

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Could not find any previous method in StackTrace implementing ActionMethodAttribute.");

            harness.AddedServicesHistory.Should().BeEmpty();
        }

        /// <summary>
        /// Test the AddOperationHistoryAsync should register the operation against the running service history.
        /// </summary>
        [Fact]
        public async Task AddOperationHistoryAsync_Pass_RegistersTheOperationAgainstTheServiceHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            await new AuditedServiceFake(harness.Service).BeginAsync();

            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);

            // Act
            await harness.Service.AddOperationHistoryAsync(context.Entry(entity));

            // Assert
            harness.AddedOperationsHistory.Should().ContainSingle();
            harness.AddedOperationsHistory[0].EntityName.Should().Be(nameof(AuditableEntityFake));
            harness.AddedOperationsHistory[0].EntityID.Should().Be(3);
            harness.AddedOperationsHistory[0].ServiceHistory.Should().BeSameAs(harness.AddedServicesHistory[0]);
        }

        /// <summary>
        /// Test the AddOperationHistoryAsync should refuse to audit before a service history was started.
        /// </summary>
        [Fact]
        public async Task AddOperationHistoryAsync_Fail_ThrowsWithoutARunningServiceHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);

            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);

            // Act
            Func<Task> act = () => harness.Service.AddOperationHistoryAsync(context.Entry(entity));

            // Assert
            await act.Should().ThrowAsync<MissingServiceHistoryException>();
            harness.AddedOperationsHistory.Should().BeEmpty();
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should project and paginate the service history rows.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_ProjectsAndPaginatesTheRows()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 1 };

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            List<ServicesHistoryListModel> rows = [.. result];
            rows.Should().ContainSingle();
            rows[0].ID.Should().Be(2);
            rows[0].Name.Should().Be("IService\\Second");
            rows[0].ChangedByName.Should().Be("Reviewer");
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should return nothing when the entity has no history.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_ReturnsNothingWithoutHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, new ListParametersModel());

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should project the operations of the service history.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_ProjectsTheOperationsOfTheServiceHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3);

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, new ListParametersModel());

            // Assert
            List<OperationsHistoryListModel> rows = [.. result];
            rows.Should().ContainSingle();
            rows[0].EntityID.Should().Be(3);
            rows[0].TableName.Should().Be(nameof(AuditableEntityFake));
            rows[0].OperationType.Should().Be("Modified");
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should paginate the projected operations.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_PaginatesTheProjectedOperations()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3, id: 20);

            IListParameters parameters = new ListParametersModel { StartRow = 1, EndRow = 5 };

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(20L);
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should return nothing when the service history did
        /// not touch the requested entity.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_ReturnsNothingWhenTheEntityWasNotTouched()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, "AnotherTable", 99);

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, new ListParametersModel());

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should refuse an unknown entity.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Fail_ThrowsForAnUnknownEntity()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            // Act
            Func<Task> act = () => harness.Service.ListEntityOperationsHistoryAsync<AuditableEntityFake>(404, 1, new ListParametersModel());

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException<AuditableEntityFake>>();
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should refuse an unknown service history.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Fail_ThrowsForAnUnknownServiceHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            // Act
            Func<Task> act = () => harness.Service.ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 404, new ListParametersModel());

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException<ServicesHistoryFake>>();
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should match the service name with LIKE.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_FiltersTheServiceNameWithLike()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            IListParameters parameters = Filters((AuditFilters.Name, "\"Second\""));

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should match the author exactly.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_FiltersTheAuthor()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            IListParameters parameters = Filters((AuditFilters.ChangedByID, "1"));

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            result.Select(x => x.ChangedByName).Should().Equal("Author");
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should treat both ends of the date range as inclusive.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_FiltersAnInclusiveDateRange()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            // The bounds are the two rows' own instants, so an exclusive comparison would return nothing.
            IListParameters parameters = Filters(
                (AuditFilters.ChangedOnFrom, "\"2024-01-01T00:00:00\""),
                (AuditFilters.ChangedOnTo, "\"2024-02-01T00:00:00\""));

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            result.Select(x => x.ID).Should().BeEquivalentTo(new[] { 1L, 2L });
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should narrow a date range to one end.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_FiltersFromADateAlone()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            IListParameters parameters = Filters((AuditFilters.ChangedOnFrom, "\"2024-01-15T00:00:00\""));

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(2L);
        }

        /// <summary>
        /// Test the ListEntityServicesHistoryAsync should combine the filters rather than pick one.
        /// </summary>
        [Fact]
        public async Task ListEntityServicesHistoryAsync_Pass_CombinesEveryFilter()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);

            // Each filter alone matches row 2; the author does not, so together they match nothing.
            IListParameters parameters = Filters(
                (AuditFilters.Name, "\"Second\""),
                (AuditFilters.ChangedByID, "1"));

            // Act
            IQueryable<ServicesHistoryListModel> result = await harness.Service
                .ListEntityServicesHistoryAsync<AuditableEntityFake>(3, parameters);

            // Assert
            result.Should().BeEmpty();
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should list what a service touched beyond the
        /// audited record unless asked otherwise.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_KeepsTheRelatedRowsByDefault()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3);
            SeedOperationsHistory(context, "RelatedTable", 77, id: 20);

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, new ListParametersModel());

            // Assert
            result.Select(x => x.ID).Should().BeEquivalentTo(new[] { 10L, 20L });
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should narrow the operations to the audited
        /// record when asked.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_NarrowsToTheAuditedRecordWhenAsked()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3);
            SeedOperationsHistory(context, "RelatedTable", 77, id: 20);

            IListParameters parameters = Filters((AuditFilters.OnlyCurrentEntity, "true"));

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, parameters);

            // Assert
            result.Select(x => x.ID).Should().Equal(10L);
        }

        /// <summary>
        /// Test the ListEntityOperationsHistoryAsync should leave the operations alone when the
        /// filter is present but off.
        /// </summary>
        [Fact]
        public async Task ListEntityOperationsHistoryAsync_Pass_KeepsTheRelatedRowsWhenTheFilterIsOff()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditServiceHarness harness = new(context);
            SeedServicesHistory(context);
            SeedOperationsHistory(context, nameof(AuditableEntityFake), 3);
            SeedOperationsHistory(context, "RelatedTable", 77, id: 20);

            IListParameters parameters = Filters((AuditFilters.OnlyCurrentEntity, "false"));

            // Act
            IQueryable<OperationsHistoryListModel> result = await harness.Service
                .ListEntityOperationsHistoryAsync<AuditableEntityFake>(3, 1, parameters);

            // Assert
            result.Select(x => x.ID).Should().BeEquivalentTo(new[] { 10L, 20L });
        }

        private static ListParametersModel Filters(params (string Key, string Json)[] filters)
        {
            ListParametersModel parameters = new();

            foreach ((string key, string json) in filters)
            {
                // A filter reaches the server as a JsonElement, and the conversion behind
                // GetFilterValue reads it as one. Building the dictionary any other way tests a
                // path no request takes.
                parameters.Filters![key] = JsonDocument.Parse(json).RootElement;
            }

            return parameters;
        }

        private static void SeedServicesHistory(DbContextFake context)
        {
            UsersFake author = new() { ID = 1, Name = "Author" };
            UsersFake reviewer = new() { ID = 2, Name = "Reviewer" };

            context.Users.AddRange(author, reviewer);
            context.AuditableEntities.Add(new AuditableEntityFake { ID = 3, Name = "Alpha", CreatedByID = 1, LastChangedByID = 1 });
            context.ServicesHistory.AddRange(
                new ServicesHistoryFake { ID = 1, Name = "IService\\First", ChangedByID = 1, ChangedOn = new DateTime(2024, 1, 1) },
                new ServicesHistoryFake { ID = 2, Name = "IService\\Second", ChangedByID = 2, ChangedOn = new DateTime(2024, 2, 1) });

            context.SaveChanges();
            context.ChangeTracker.Clear();
        }

        private static void SeedOperationsHistory(DbContextFake context, string tableName, long entityID, long id = 10)
        {
            context.OperationsHistory.Add(new OperationsHistoryFake
            {
                ID = id,
                ServiceHistoryID = 1,
                TableName = tableName,
                EntityName = tableName,
                EntityID = entityID,
                OperationType = "Modified",
            });

            context.SaveChanges();
            context.ChangeTracker.Clear();
        }

        private sealed class AuditServiceHarness
        {
            internal AuditServiceHarness(DbContextFake context)
            {
                IMapper mapper = MapperFakeFactory.Create();

                SecurityHandler = Substitute.For<ISecurityHandler>();

                OperationsHistoryRepository = Substitute.For<IOperationsHistoryRepository<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>>();
                OperationsHistoryRepository
                    .AddOperationHistoryAsync(Arg.Do<OperationsHistoryFake>(AddedOperationsHistory.Add))
                    .Returns(Task.CompletedTask);
                OperationsHistoryRepository
                    .ListOperations(Arg.Any<long>())
                    .Returns(x => context.OperationsHistory.Where(y => y.ServiceHistoryID == x.ArgAt<long>(0)));

                ServicesHistoryRepository = Substitute.For<IServicesHistoryRepository<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>>();
                ServicesHistoryRepository
                    .AddServiceHistoryAsync(Arg.Do<ServicesHistoryFake>(AddedServicesHistory.Add))
                    .Returns(Task.CompletedTask);
                ServicesHistoryRepository
                    .ListServicesAsync<AuditableEntityFake>(Arg.Any<long>())
                    .Returns(Task.FromResult<IQueryable<ServicesHistoryFake>>(context.ServicesHistory.OrderBy(x => x.ID)));

                Service = new AuditServiceDefault<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(
                    context,
                    mapper,
                    OperationsHistoryRepository,
                    SecurityHandler,
                    ServicesHistoryRepository);
            }

            internal List<OperationsHistoryFake> AddedOperationsHistory { get; } = [];

            internal List<ServicesHistoryFake> AddedServicesHistory { get; } = [];

            internal IOperationsHistoryRepository<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long> OperationsHistoryRepository { get; }

            internal ISecurityHandler SecurityHandler { get; }

            internal IAuditService<UsersFake, long> Service { get; }

            internal IServicesHistoryRepository<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long> ServicesHistoryRepository { get; }
        }
    }
}
