using ZWebAPI.Interfaces;

namespace ZWebAPI.Models
{
    /// <summary>
    /// Parameters model to filter summaries.
    /// </summary>
    /// <seealso cref="ZWebAPI.Interfaces.ISummaryParameters" />
    public class SummaryParametersModel : ISummaryParameters
    {
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>
        /// The filters.
        /// </value>
        public IDictionary<string, object>? Filters { get; set; } = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
    }
}