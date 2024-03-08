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
        /// <inheritdoc />
        public int EndRow { get; set; }

        /// <inheritdoc />
        public IDictionary<string, string> Sort { get; set; } = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

        /// <inheritdoc />
        public int StartRow { get; set; }
    }
}