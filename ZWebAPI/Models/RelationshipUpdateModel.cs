namespace ZWebAPI.Models
{
    /// <summary>
    /// Update model for entity relationships.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class RelationshipUpdateModel<TKey>
    {
        /// <summary>
        /// Gets or sets the ids to add.
        /// </summary>
        /// <value>
        /// The ids to add.
        /// </value>
        public IEnumerable<TKey> IDsToAdd { get; set; } = Enumerable.Empty<TKey>();

        /// <summary>
        /// Gets or sets the ids to remove.
        /// </summary>
        /// <value>
        /// The ids to remove.
        /// </value>
        public IEnumerable<TKey> IDsToRemove { get; set; } = Enumerable.Empty<TKey>();
    }
}
