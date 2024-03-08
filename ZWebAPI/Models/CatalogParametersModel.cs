using ZWebAPI.Interfaces;

namespace ZWebAPI.Models
{
    /// <summary>
    /// Parameters model for catalogs.
    /// </summary>
    /// <seealso cref="ZWebAPI.Interfaces.ICatalogParameters" />
    public class CatalogParametersModel : SummaryParametersModel, ICatalogParameters
    {
        /// <inheritdoc />
        public string? Criteria { get; set; }

        /// <inheritdoc />
        public int MaxResults { get; set; }
    }
}
