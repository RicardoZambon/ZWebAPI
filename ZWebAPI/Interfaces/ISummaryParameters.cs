namespace ZWebAPI.Interfaces
{
    /// <summary>
    /// Interface for summary parameters.
    /// </summary>
    public interface ISummaryParameters
    {
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        IDictionary<string, object>? Filters { get; set; }
    }
}