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