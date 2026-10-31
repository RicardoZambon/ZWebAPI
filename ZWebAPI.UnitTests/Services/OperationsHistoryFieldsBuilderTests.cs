using System.Text.Json;
using ZWebAPI.Models.Audit.OperationHistory;
using ZWebAPI.Services;
using static ZWebAPI.Services.OperationsHistoryFieldsBuilder;

namespace ZWebAPI.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Services.OperationsHistoryFieldsBuilder"/>.
    /// </summary>
    public class OperationsHistoryFieldsBuilderTests
    {
        private static readonly IReadOnlyList<FieldDescriptor> Descriptors =
        [
            new FieldDescriptor("ID", OperationsHistoryFieldTypes.Key, true),
            new FieldDescriptor("Name", OperationsHistoryFieldTypes.Text, false),
            new FieldDescriptor("Age", OperationsHistoryFieldTypes.Number, false),
        ];

        /// <summary>
        /// Test the Build should mark the fields an update wrote as changed, with both values.
        /// </summary>
        [Fact]
        public void Build_Pass_MarksTheFieldsAnUpdateWrote()
        {
            // Arrange
            RecordedOperation update = Operation(10, "Modified", """{"Name":"Alpha"}""", """{"Name":"Beta"}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(update, [update], null, Descriptors);

            // Assert
            OperationsHistoryFieldModel name = fields.Single(x => x.Name == "Name");
            name.IsChanged.Should().BeTrue();
            name.OldValue!.Value.GetString().Should().Be("Alpha");
            name.NewValue!.Value.GetString().Should().Be("Beta");
        }

        /// <summary>
        /// Test the Build should take an untouched field from the last earlier operation that wrote it.
        /// </summary>
        [Fact]
        public void Build_Pass_TakesAnUntouchedFieldFromTheLastEarlierWrite()
        {
            // Arrange
            RecordedOperation created = Operation(1, "Added", "{}", """{"ID":3,"Name":"Alpha","Age":30}""");
            RecordedOperation aged = Operation(5, "Modified", """{"Age":30}""", """{"Age":31}""");
            RecordedOperation renamed = Operation(10, "Modified", """{"Name":"Alpha"}""", """{"Name":"Beta"}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(renamed, [created, aged, renamed], Current(age: 40), Descriptors);

            // Assert
            OperationsHistoryFieldModel age = fields.Single(x => x.Name == "Age");
            age.IsChanged.Should().BeFalse();
            age.OldValue!.Value.GetInt32().Should().Be(31);
            age.NewValue!.Value.GetInt32().Should().Be(31);
        }

        /// <summary>
        /// Test the Build should take an untouched field from the first later operation that changed
        /// it, when nothing earlier wrote it -- a record created before the audit existed.
        /// </summary>
        [Fact]
        public void Build_Pass_TakesAnUntouchedFieldFromTheFirstLaterChange()
        {
            // Arrange
            RecordedOperation renamed = Operation(10, "Modified", """{"Name":"Alpha"}""", """{"Name":"Beta"}""");
            RecordedOperation aged = Operation(20, "Modified", """{"Age":30}""", """{"Age":31}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(renamed, [renamed, aged], Current(age: 31), Descriptors);

            // Assert
            fields.Single(x => x.Name == "Age").NewValue!.Value.GetInt32().Should().Be(30);
        }

        /// <summary>
        /// Test the Build should take a field nothing ever changed from the record as it is now.
        /// </summary>
        [Fact]
        public void Build_Pass_TakesAFieldNothingChangedFromTheCurrentRecord()
        {
            // Arrange
            RecordedOperation renamed = Operation(10, "Modified", """{"Name":"Alpha"}""", """{"Name":"Beta"}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(renamed, [renamed], Current(age: 40), Descriptors);

            // Assert
            fields.Single(x => x.Name == "Age").OldValue!.Value.GetInt32().Should().Be(40);
            fields.Single(x => x.Name == "ID").NewValue!.Value.GetInt64().Should().Be(3);
        }

        /// <summary>
        /// Test the Build should leave a field it cannot rebuild without a value rather than guess.
        /// </summary>
        [Fact]
        public void Build_Pass_LeavesAnUnknownFieldEmpty()
        {
            // Arrange
            RecordedOperation renamed = Operation(10, "Modified", """{"Name":"Alpha"}""", """{"Name":"Beta"}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(renamed, [renamed], null, Descriptors);

            // Assert
            OperationsHistoryFieldModel age = fields.Single(x => x.Name == "Age");
            age.OldValue.Should().BeNull();
            age.NewValue.Should().BeNull();
        }

        /// <summary>
        /// Test the Build should give a creation new values only.
        /// </summary>
        [Fact]
        public void Build_Pass_GivesACreationNewValuesOnly()
        {
            // Arrange
            RecordedOperation created = Operation(1, "Added", "{}", """{"ID":3,"Name":"Alpha","Age":30}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(created, [created], Current(age: 40), Descriptors);

            // Assert
            fields.Should().OnlyContain(x => x.IsChanged && x.OldValue == null && x.NewValue != null);
            fields.Single(x => x.Name == "Age").NewValue!.Value.GetInt32().Should().Be(30);
        }

        /// <summary>
        /// Test the Build should give a deletion old values only.
        /// </summary>
        [Fact]
        public void Build_Pass_GivesADeletionOldValuesOnly()
        {
            // Arrange
            RecordedOperation deleted = Operation(30, "Deleted", """{"ID":3,"Name":"Beta","Age":31}""", "{}");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(deleted, [deleted], null, Descriptors);

            // Assert
            fields.Should().OnlyContain(x => x.IsChanged && x.OldValue != null && x.NewValue == null);
        }

        /// <summary>
        /// Test the Build should keep the declared fields in order and their type and system flag,
        /// then add the ones the operation names that the model does not know.
        /// </summary>
        [Fact]
        public void Build_Pass_KeepsTheDeclaredFieldsThenAddsUnknownOnes()
        {
            // Arrange
            RecordedOperation update = Operation(10, "Modified", """{"Legacy":"x"}""", """{"Legacy":"y"}""");

            // Act
            IList<OperationsHistoryFieldModel> fields = Build(update, [update], null, Descriptors);

            // Assert
            fields.Select(x => x.Name).Should().Equal("ID", "Name", "Age", "Legacy");
            fields[0].IsSystem.Should().BeTrue();
            fields[0].Type.Should().Be(OperationsHistoryFieldTypes.Key);
            fields[3].Type.Should().Be(OperationsHistoryFieldTypes.Other);
            fields[3].IsChanged.Should().BeTrue();
        }

        /// <summary>
        /// Test the RecordedOperation.From should read missing or malformed values as empty.
        /// </summary>
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not json")]
        [InlineData("[1,2]")]
        public void RecordedOperationFrom_Pass_ReadsUnusableValuesAsEmpty(string? json)
        {
            // Act
            RecordedOperation operation = RecordedOperation.From(1, "Modified", json, json);

            // Assert
            operation.OldValues.Should().BeEmpty();
            operation.NewValues.Should().BeEmpty();
        }

        private static IReadOnlyDictionary<string, JsonElement> Current(int age)
        {
            return new Dictionary<string, JsonElement>
            {
                ["ID"] = JsonSerializer.SerializeToElement(3L),
                ["Name"] = JsonSerializer.SerializeToElement("Beta"),
                ["Age"] = JsonSerializer.SerializeToElement(age),
            };
        }

        private static RecordedOperation Operation(long id, string type, string oldValues, string newValues)
        {
            return RecordedOperation.From(id, type, oldValues, newValues);
        }
    }
}
