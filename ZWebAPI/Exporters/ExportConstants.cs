namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Shared constants for the list-export feature.
    /// </summary>
    public static class ExportConstants
    {
        /// <summary>
        /// Maximum number of rows allowed in a single export request.
        /// Requests exceeding this throw <see cref="Exceptions.ExportRowLimitExceededException"/>.
        /// </summary>
        public const int MaxExportRows = 50_000;
    }
}
