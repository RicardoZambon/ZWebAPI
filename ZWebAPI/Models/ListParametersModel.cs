using ZWebAPI.Interfaces;

namespace ZWebAPI.Models
{
    /// <summary>
    /// Parameters model to filter or sort lists.
    /// </summary>
    /// <seealso cref="ZWebAPI.Models.SummaryParametersModel" />
    /// <seealso cref="ZWebAPI.Interfaces.IListParameters" />
    public class ListParametersModel : SummaryParametersModel, IListParameters
    {
        /// <summary>
        /// Gets or sets the start row.
        /// </summary>
        /// <value>
        /// The start row.
        /// </value>
        public int StartRow { get; set; }

        /// <summary>
        /// Gets or sets the end row.
        /// </summary>
        /// <value>
        /// The end row.
        /// </value>
        public int EndRow { get; set; }

        /// <summary>
        /// Gets or sets the sort.
        /// </summary>
        /// <value>
        /// The sort.
        /// </value>
        public IDictionary<string, string> Sort { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
    }
}