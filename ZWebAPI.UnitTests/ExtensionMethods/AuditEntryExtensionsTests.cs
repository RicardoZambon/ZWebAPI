using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ZDatabase.Entries;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.ExtensionMethods
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.ExtensionMethods.AuditEntryExtensions"/>.
    /// </summary>
    public class AuditEntryExtensionsTests
    {
        /// <summary>
        /// Test the GenerateOperationsHistory should link the operation to the running service history.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_LinksTheRunningServiceHistory()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);

            ServicesHistoryFake servicesHistory = new() { ID = 7, Name = "IService\\Method" };
            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(servicesHistory);

            // Assert
            result.ServiceHistoryID.Should().Be(7);
            result.ServiceHistory.Should().BeSameAs(servicesHistory);
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should describe the audited entity.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_DescribesTheAuditedEntity()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.EntityName.Should().Be(nameof(AuditableEntityFake));
            result.TableName.Should().Be(nameof(AuditableEntityFake));
            result.EntityID.Should().Be(3);
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should leave the entity identifier empty for a type that
        /// is not a database entity.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_LeavesTheEntityIdentifierEmptyForANonEntity()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            NonEntityFake entity = new() { ID = 3, Description = "Alpha" };
            context.NonEntities.Add(entity);

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.EntityName.Should().Be(nameof(NonEntityFake));
            result.EntityID.Should().BeNull();
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should record an insertion with its new values only.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_RecordsAnInsertionWithNewValuesOnly()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.OperationType.Should().Be(nameof(EntityState.Added));
            result.OldValues.Should().Be("{}");
            Read(result.NewValues).Should().ContainKey(nameof(AuditableEntityFake.Name));
            Read(result.NewValues)[nameof(AuditableEntityFake.Name)].Should().Be("Alpha");
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should record an update with the changed values on both sides.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_RecordsAnUpdateWithBothSides()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);
            context.SaveChanges();

            entity.Name = "Beta";
            context.ChangeTracker.DetectChanges();

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.OperationType.Should().Be(nameof(EntityState.Modified));
            Read(result.OldValues).Should().ContainSingle();
            Read(result.OldValues)[nameof(AuditableEntityFake.Name)].Should().Be("Alpha");
            Read(result.NewValues)[nameof(AuditableEntityFake.Name)].Should().Be("Beta");
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should record a deletion with its old values only.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_RecordsADeletionWithOldValuesOnly()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);
            context.SaveChanges();
            context.AuditableEntities.Remove(entity);

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.OperationType.Should().Be(nameof(EntityState.Deleted));
            Read(result.OldValues)[nameof(AuditableEntityFake.Name)].Should().Be("Alpha");
            result.NewValues.Should().Be("{}");
        }

        /// <summary>
        /// Test the GenerateOperationsHistory should record nothing on either side for an untouched entity.
        /// </summary>
        [Fact]
        public void GenerateOperationsHistory_Pass_RecordsNothingForAnUntouchedEntity()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();
            AuditableEntityFake entity = new() { ID = 3, Name = "Alpha" };
            context.AuditableEntities.Add(entity);
            context.SaveChanges();

            AuditEntry auditEntry = new(context.Entry(entity));

            // Act
            OperationsHistoryFake result = auditEntry.GenerateOperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>(new ServicesHistoryFake());

            // Assert
            result.OperationType.Should().Be(nameof(EntityState.Unchanged));
            result.OldValues.Should().Be("{}");
            result.NewValues.Should().Be("{}");
        }

        private static Dictionary<string, object?> Read(string? json)
        {
            return JsonSerializer.Deserialize<Dictionary<string, object?>>(json ?? "{}")!
                .ToDictionary(x => x.Key, x => x.Value is JsonElement element && element.ValueKind == JsonValueKind.String ? element.GetString() : x.Value);
        }
    }
}
