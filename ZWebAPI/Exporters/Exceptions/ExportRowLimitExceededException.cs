using System;

namespace ZWebAPI.Exporters.Exceptions
{
    /// <summary>
    /// Thrown when a list export request would exceed <see cref="ExportConstants.MaxExportRows"/>.
    /// Controllers map this exception to HTTP 413 Payload Too Large.
    /// </summary>
    public sealed class ExportRowLimitExceededException : Exception
    {
        #region Properties
        /// <summary>The configured row limit.</summary>
        public int MaxRows { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportRowLimitExceededException"/> class.
        /// </summary>
        /// <param name="maxRows">The configured row limit.</param>
        public ExportRowLimitExceededException(int maxRows)
            : base($"The export result exceeds the configured limit of {maxRows} rows.")
        {
            MaxRows = maxRows;
        }
        #endregion
    }
}
