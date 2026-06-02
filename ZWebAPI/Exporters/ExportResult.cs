namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Result produced by <see cref="IListExporter"/> — a file payload with its MIME metadata.
    /// </summary>
    public sealed class ExportResult
    {
        #region Properties
        /// <summary>File bytes.</summary>
        public byte[] Content { get; }

        /// <summary>MIME content type.</summary>
        public string ContentType { get; }

        /// <summary>Full file name including extension.</summary>
        public string FileName { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportResult"/> class.
        /// </summary>
        /// <param name="content">File bytes.</param>
        /// <param name="contentType">MIME content type.</param>
        /// <param name="fileName">Full file name including extension.</param>
        public ExportResult(byte[] content, string contentType, string fileName)
        {
            Content = content;
            ContentType = contentType;
            FileName = fileName;
        }
        #endregion
    }
}
