namespace ZWebAPI.Interfaces
{
    /// <summary>
    /// Interface for catalog parameters.
    /// </summary>
    public interface ICatalogParameters : ISummaryParameters
    {
        /// <summary>
        /// Gets or sets the criteria.
        /// </summary>
        /// <value>
        /// The criteria.
        /// </value>
        public string? Criteria { get; set; }

        /// <summary>
        /// Gets or sets the maximum results.
        /// </summary>
        /// <value>
        /// The maximum results.
        /// </value>
        public int MaxResults { get; set; }
    }
}