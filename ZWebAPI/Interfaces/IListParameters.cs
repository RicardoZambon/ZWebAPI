namespace ZWebAPI.Interfaces
{
    /// <summary>
    /// Interface for list parameters.
    /// </summary>
    public interface IListParameters : ISummaryParameters
    {
        /// <summary>
        /// Gets or sets the end row.
        /// </summary>
        /// <value>
        /// The end row.
        /// </value>
        int EndRow { get; set; }

        /// <summary>
        /// Gets or sets the sort.
        /// </summary>
        /// <value>
        /// The sort.
        /// </value>
        IDictionary<string, string> Sort { get; set; }

        /// <summary>
        /// Gets or sets the start row.
        /// </summary>
        /// <value>
        /// The start row.
        /// </value>
        int StartRow { get; set; }
    }
}