namespace ZWebAPI.Models.Catalog
{
    /// <summary>
    /// Model for catalog results.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class CatalogResultModel<TKey>
        where TKey : struct
    {
        /// <summary>
        /// Gets or sets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public IEnumerable<CatalogEntryModel<TKey>> Entries { get; set; } = Enumerable.Empty<CatalogEntryModel<TKey>>();

        /// <summary>
        /// Gets or sets a value indicating whether should use criteria to filter the entries.
        /// </summary>
        /// <value>
        ///   <c>true</c> if should use criteria; otherwise, <c>false</c>.
        /// </value>
        public bool ShouldUseCriteria { get; set; }
    }
}
