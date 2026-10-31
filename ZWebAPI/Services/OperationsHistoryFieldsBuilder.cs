using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Text.Json;
using ZDatabase.Entities;
using ZDatabase.Entities.Audit;
using ZDatabase.Interfaces;
using ZWebAPI.Models.Audit.OperationHistory;

namespace ZWebAPI.Services
{
    /// <summary>
    /// Rebuilds every field of an audited record, before and after one operation.
    /// </summary>
    /// <remarks>
    /// An operation stores only what it wrote: the whole record for a creation or a deletion, but
    /// the changed fields alone for an update. A field an update did not touch kept the value it
    /// had, which is the value the last earlier operation wrote, or else the value the first later
    /// operation found, or else -- when nothing ever touched it -- the value the record holds now.
    /// That holds as long as every write went through the audit: a field changed by a raw SQL
    /// script is rebuilt from the wrong value.
    /// </remarks>
    internal static class OperationsHistoryFieldsBuilder
    {
        #region Public methods

        /// <summary>
        /// Fills <see cref="OperationsHistoryListModel.Fields"/> on every operation of a list.
        /// </summary>
        /// <typeparam name="TOperationsHistory">The type of the operations history.</typeparam>
        /// <param name="dbContext">The database context.</param>
        /// <param name="operations">The operations to fill.</param>
        /// <returns>A task that completes once every operation is filled.</returns>
        internal static async Task FillFieldsAsync<TOperationsHistory>(IDbContext dbContext, IList<OperationsHistoryListModel> operations)
            where TOperationsHistory : OperationsHistoryBase
        {
            // One history and one current record per audited record, however many of its operations
            // the list shows.
            foreach (IGrouping<(string? TableName, long? EntityID), OperationsHistoryListModel> record in operations.GroupBy(x => (x.TableName, x.EntityID)))
            {
                IReadOnlyList<FieldDescriptor> descriptors = DescribeFields(dbContext.Model, record.Key.TableName);

                IReadOnlyList<RecordedOperation> history = record.Key.EntityID is long entityID && record.Key.TableName is string tableName
                    ? await LoadHistoryAsync<TOperationsHistory>(dbContext, tableName, entityID)
                    : [];

                IReadOnlyDictionary<string, JsonElement>? current = record.Key.EntityID is long id
                    ? await LoadCurrentValuesAsync(dbContext, record.Key.TableName, id)
                    : null;

                foreach (OperationsHistoryListModel operation in record)
                {
                    RecordedOperation recorded = history.FirstOrDefault(x => x.ID == operation.ID)
                        ?? RecordedOperation.From(operation.ID, operation.OperationType, operation.OldValues, operation.NewValues);

                    operation.Fields = Build(recorded, history, current, descriptors);
                }
            }
        }

        /// <summary>
        /// Rebuilds the fields of one operation.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <param name="history">Every operation on the same record, in the order they happened.</param>
        /// <param name="current">The record's values now, or <c>null</c> when it no longer exists.</param>
        /// <param name="descriptors">The record's fields as the model declares them, in order.</param>
        /// <returns>The fields: the declared ones first, then any the operation names that the model does not.</returns>
        internal static IList<OperationsHistoryFieldModel> Build(
            RecordedOperation operation,
            IReadOnlyList<RecordedOperation> history,
            IReadOnlyDictionary<string, JsonElement>? current,
            IReadOnlyList<FieldDescriptor> descriptors)
        {
            List<FieldDescriptor> fields = [.. descriptors];
            foreach (string name in operation.OldValues.Keys.Concat(operation.NewValues.Keys))
            {
                if (!fields.Any(x => x.Name == name))
                {
                    fields.Add(new FieldDescriptor(name, OperationsHistoryFieldTypes.Other, false));
                }
            }

            return fields.Select(field => BuildField(operation, history, current, field)).ToList();
        }

        #endregion

        #region Private methods

        private static OperationsHistoryFieldModel BuildField(
            RecordedOperation operation,
            IReadOnlyList<RecordedOperation> history,
            IReadOnlyDictionary<string, JsonElement>? current,
            FieldDescriptor field)
        {
            bool hasOld = operation.OldValues.TryGetValue(field.Name, out JsonElement oldValue);
            bool hasNew = operation.NewValues.TryGetValue(field.Name, out JsonElement newValue);

            OperationsHistoryFieldModel model = new()
            {
                IsSystem = field.IsSystem,
                Name = field.Name,
                Type = field.Type,
            };

            switch (operation.OperationType)
            {
                case nameof(EntityState.Added):
                    model.IsChanged = hasNew;
                    model.NewValue = hasNew ? newValue : ValueAt(field.Name, operation, history, current);
                    break;

                case nameof(EntityState.Deleted):
                    model.IsChanged = hasOld;
                    model.OldValue = hasOld ? oldValue : ValueAt(field.Name, operation, history, current);
                    break;

                default:
                    if (hasOld || hasNew)
                    {
                        model.IsChanged = true;
                        model.OldValue = hasOld ? oldValue : null;
                        model.NewValue = hasNew ? newValue : null;
                    }
                    else
                    {
                        model.OldValue = model.NewValue = ValueAt(field.Name, operation, history, current);
                    }
                    break;
            }

            return model;
        }

        private static IReadOnlyList<FieldDescriptor> DescribeFields(IModel model, string? tableName)
        {
            if (tableName is null)
            {
                return [];
            }

            // Every type mapped to the table, the root first: a hierarchy sharing one table spreads
            // its columns over all of them.
            IEnumerable<IEntityType> entityTypes = model.GetEntityTypes()
                .Where(x => string.Equals(x.GetTableName(), tableName, StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.BaseType is null ? 0 : 1);

            List<FieldDescriptor> descriptors = [];
            foreach (IProperty property in entityTypes.SelectMany(x => x.GetDeclaredProperties()))
            {
                if (!descriptors.Any(x => x.Name == property.Name))
                {
                    descriptors.Add(new FieldDescriptor(property.Name, GetFieldType(property), IsSystemField(property)));
                }
            }

            return descriptors;
        }

        private static string GetFieldType(IProperty property)
        {
            if (property.IsPrimaryKey() || property.IsForeignKey())
            {
                return OperationsHistoryFieldTypes.Key;
            }

            Type type = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;

            if (type.IsEnum)
            {
                return OperationsHistoryFieldTypes.Enum;
            }

            return Type.GetTypeCode(type) switch
            {
                TypeCode.Boolean => OperationsHistoryFieldTypes.Boolean,
                TypeCode.Byte or TypeCode.SByte or TypeCode.Int16 or TypeCode.UInt16 or TypeCode.Int32 or TypeCode.UInt32
                    or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.Single or TypeCode.Double or TypeCode.Decimal => OperationsHistoryFieldTypes.Number,
                TypeCode.Char or TypeCode.String => OperationsHistoryFieldTypes.Text,
                TypeCode.DateTime => OperationsHistoryFieldTypes.Date,
                _ when type == typeof(DateTimeOffset) || type == typeof(DateOnly) => OperationsHistoryFieldTypes.Date,
                _ when type == typeof(TimeSpan) || type == typeof(TimeOnly) => OperationsHistoryFieldTypes.Time,
                _ when type == typeof(Guid) => OperationsHistoryFieldTypes.Text,
                _ => OperationsHistoryFieldTypes.Other,
            };
        }

        private static bool IsSystemField(IProperty property)
        {
            // The key, the concurrency token, and whatever the database library's own base classes
            // declare -- the audit columns and the soft-delete flag -- are bookkeeping, not data.
            return property.IsPrimaryKey()
                || property.IsConcurrencyToken
                || property.PropertyInfo?.DeclaringType?.Assembly == typeof(Entity).Assembly;
        }

        private static async Task<IReadOnlyDictionary<string, JsonElement>?> LoadCurrentValuesAsync(IDbContext dbContext, string? tableName, long entityID)
        {
            if (dbContext is not DbContext context || tableName is null)
            {
                return null;
            }

            IEntityType? entityType = context.Model.GetEntityTypes()
                .FirstOrDefault(x => x.BaseType is null && string.Equals(x.GetTableName(), tableName, StringComparison.OrdinalIgnoreCase));

            IProperty? key = entityType?.FindPrimaryKey()?.Properties is [IProperty single] ? single : null;
            if (entityType is null || key is null)
            {
                return null;
            }

            object keyValue = Convert.ChangeType(entityID, Nullable.GetUnderlyingType(key.ClrType) ?? key.ClrType);
            if (await context.FindAsync(entityType.ClrType, keyValue) is not object entity)
            {
                return null;
            }

            return context.Entry(entity).Properties
                .ToDictionary(x => x.Metadata.Name, x => JsonSerializer.SerializeToElement(x.CurrentValue));
        }

        private static async Task<IReadOnlyList<RecordedOperation>> LoadHistoryAsync<TOperationsHistory>(IDbContext dbContext, string tableName, long entityID)
            where TOperationsHistory : OperationsHistoryBase
        {
            List<TOperationsHistory> operations = await dbContext.Set<TOperationsHistory>()
                .AsNoTracking()
                .Where(x => x.TableName == tableName && x.EntityID == entityID)
                .OrderBy(x => x.ID)
                .ToListAsync();

            return operations
                .Select(x => RecordedOperation.From(x.ID, x.OperationType, x.OldValues, x.NewValues))
                .ToList();
        }

        private static JsonElement? ValueAt(
            string name,
            RecordedOperation operation,
            IReadOnlyList<RecordedOperation> history,
            IReadOnlyDictionary<string, JsonElement>? current)
        {
            for (int i = history.Count - 1; i >= 0; i--)
            {
                if (history[i].ID < operation.ID && history[i].NewValues.TryGetValue(name, out JsonElement earlier))
                {
                    return earlier;
                }
            }

            foreach (RecordedOperation later in history)
            {
                if (later.ID > operation.ID && later.OldValues.TryGetValue(name, out JsonElement found))
                {
                    return found;
                }
            }

            return current is not null && current.TryGetValue(name, out JsonElement now) ? now : null;
        }

        #endregion

        #region Nested types

        /// <summary>
        /// A field of the audited record, as the model declares it.
        /// </summary>
        /// <param name="Name">The property name.</param>
        /// <param name="Type">One of the <see cref="OperationsHistoryFieldTypes"/> values.</param>
        /// <param name="IsSystem">Whether the field is bookkeeping rather than data.</param>
        internal sealed record FieldDescriptor(string Name, string Type, bool IsSystem);

        /// <summary>
        /// An operation with its stored values parsed.
        /// </summary>
        /// <param name="ID">The operation identifier.</param>
        /// <param name="OperationType">The entity state the operation recorded.</param>
        /// <param name="OldValues">The values before the operation, by property name.</param>
        /// <param name="NewValues">The values after the operation, by property name.</param>
        internal sealed record RecordedOperation(
            long ID,
            string? OperationType,
            IReadOnlyDictionary<string, JsonElement> OldValues,
            IReadOnlyDictionary<string, JsonElement> NewValues)
        {
            /// <summary>
            /// Parses an operation's stored values. A value that is missing or is not a JSON object
            /// reads as empty.
            /// </summary>
            /// <param name="id">The operation identifier.</param>
            /// <param name="operationType">The entity state the operation recorded.</param>
            /// <param name="oldValues">The stored old values.</param>
            /// <param name="newValues">The stored new values.</param>
            /// <returns>The parsed operation.</returns>
            internal static RecordedOperation From(long id, string? operationType, string? oldValues, string? newValues)
            {
                return new RecordedOperation(id, operationType, Parse(oldValues), Parse(newValues));
            }

            private static IReadOnlyDictionary<string, JsonElement> Parse(string? json)
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new Dictionary<string, JsonElement>();
                }

                try
                {
                    using JsonDocument document = JsonDocument.Parse(json);
                    return document.RootElement.ValueKind == JsonValueKind.Object
                        ? document.RootElement.EnumerateObject().ToDictionary(x => x.Name, x => x.Value.Clone())
                        : new Dictionary<string, JsonElement>();
                }
                catch (JsonException)
                {
                    return new Dictionary<string, JsonElement>();
                }
            }
        }

        #endregion
    }
}
