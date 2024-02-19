namespace ZWebAPI.Models.Audit.ServiceHistory
{
    /// <summary>
    /// List model for <see cref="ZDatabase.Entities.Audit.ServicesHistory{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/>.
    /// </summary>
    public class ServicesHistoryListModel
    {
        /// <summary>
        /// Gets or sets the changed on.
        /// </summary>
        /// <value>
        /// The changed on.
        /// </value>
        public DateTime ChangedOn { get; set; }

        /// <summary>
        /// Gets or sets the name of the changed by.
        /// </summary>
        /// <value>
        /// The name of the changed by.
        /// </value>
        public string? ChangedByName { get; set; }

        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public long ID { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string? Name { get; set; }
    }
}