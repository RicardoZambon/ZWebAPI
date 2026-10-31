using AutoMapper.Configuration.Annotations;

namespace ZWebAPI.Models.Audit.OperationHistory
{
    /// <summary>
    /// List model for <see cref="ZDatabase.Entities.Audit.OperationsHistory{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/>.
    /// </summary>
    public class OperationsHistoryListModel
    {
        /// <summary>
        /// Gets or sets the entity identifier.
        /// </summary>
        /// <value>
        /// The entity identifier.
        /// </value>
        public long? EntityID { get; set; }

        /// <summary>
        /// Gets or sets the name of the entity.
        /// </summary>
        /// <value>
        /// The name of the entity.
        /// </value>
        public string? EntityName { get; set; }

        /// <summary>
        /// Gets or sets every field of the record, before and after the operation.
        /// </summary>
        /// <remarks>
        /// <see cref="NewValues"/> and <see cref="OldValues"/> hold only what the operation wrote,
        /// which for an update is the changed fields alone. These are rebuilt from the record's
        /// whole history, so an update shows the full record too. Not mapped: the audit service
        /// fills it after the query.
        /// </remarks>
        /// <value>
        /// The fields, or <c>null</c> when they could not be rebuilt.
        /// </value>
        [Ignore]
        public IList<OperationsHistoryFieldModel>? Fields { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long ID { get; set; }

        /// <summary>
        /// Creates new values.
        /// </summary>
        /// <value>
        /// The new values.
        /// </value>
        public string? NewValues { get; set; }

        /// <summary>
        /// Gets or sets the old values.
        /// </summary>
        /// <value>
        /// The old values.
        /// </value>
        public string? OldValues { get; set; }

        /// <summary>
        /// Gets or sets the type of the operation.
        /// </summary>
        /// <value>
        /// The type of the operation.
        /// </value>
        public string? OperationType { get; set; }

        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        public string? TableName { get; set; }
    }
}