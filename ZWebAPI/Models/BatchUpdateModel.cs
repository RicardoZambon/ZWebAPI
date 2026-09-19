namespace ZWebAPI.Models
{
    /// <summary>
    /// Batch update model for entities.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class BatchUpdateModel<TEntity, TKey>
        where TEntity : class
    {
        /// <summary>
        /// Gets or sets the entities to delete.
        /// </summary>
        /// <value>
        /// The entities to delete.
        /// </value>
        public TKey[]? EntitiesToDelete { get; set; }

        /// <summary>
        /// Gets or sets the entities to add or update.
        /// </summary>
        /// <value>
        /// The entities to add or update.
        /// </value>
        public TEntity[]? EntitiesToInsertOrUpdate { get; set; }
    }
}
