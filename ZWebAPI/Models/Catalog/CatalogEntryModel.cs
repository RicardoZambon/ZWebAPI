namespace ZWebAPI.Models.Catalog
{
    /// <summary>
    /// Entry model for catalog entries.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public class CatalogEntryModel<TKey>
        where TKey : struct
    {
        /// <summary>
        /// Gets or sets the display.
        /// </summary>
        /// <value>
        /// The display.
        /// </value>
        public string? Display { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public TKey Value { get; set; }
    }
}