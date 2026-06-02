using ZWebAPI.Exporters.Formats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Default <see cref="IListExporter"/> implementation. Resolves column metadata via
    /// <see cref="ColumnMetadataResolver"/> and dispatches to the format-specific exporter.
    /// </summary>
    public sealed class ListExporter : IListExporter
    {
        #region Variables
        private readonly CsvExporter csvExporter = new();
        private readonly MhtmlExporter mhtmlExporter = new();
        private readonly PdfExporter pdfExporter = new();
        private readonly XlsxExporter xlsxExporter = new();
        private readonly XmlExporter xmlExporter = new();
        #endregion

        #region Public methods
        /// <inheritdoc />
        public ExportResult Export<T>(IEnumerable<T> rows, ExportFormat format, string fileBaseName)
        {
            IReadOnlyList<ExportColumn> columns = ColumnMetadataResolver.Resolve(typeof(T));
            return Export(rows.Cast<object>(), columns, format, fileBaseName);
        }

        /// <inheritdoc />
        public ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, ExportFormat format, string fileBaseName)
        {
            ArgumentNullException.ThrowIfNull(rows);
            ArgumentNullException.ThrowIfNull(columns);
            ArgumentException.ThrowIfNullOrWhiteSpace(fileBaseName);

            List<object> materialized = rows.ToList();

            return format switch
            {
                ExportFormat.Xlsx => xlsxExporter.Export(materialized, columns, fileBaseName),
                ExportFormat.Csv => csvExporter.Export(materialized, columns, fileBaseName),
                ExportFormat.Pdf => pdfExporter.Export(materialized, columns, fileBaseName),
                ExportFormat.Xml => xmlExporter.Export(materialized, columns, fileBaseName),
                ExportFormat.Mhtml => mhtmlExporter.Export(materialized, columns, fileBaseName),
                _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported export format."),
            };
        }
        #endregion
    }
}
