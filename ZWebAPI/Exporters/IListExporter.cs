using System.Collections.Generic;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Generic list exporter — converts a row collection into a file payload for any supported
    /// <see cref="ExportFormat"/>. Column metadata is either reflected from the row type's
    /// public properties (optionally annotated with <see cref="ExportColumnAttribute"/>) or
    /// provided explicitly by the caller.
    /// </summary>
    public interface IListExporter
    {
        /// <summary>
        /// Exports <paramref name="rows"/> using columns reflected from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">Row/model type.</typeparam>
        /// <param name="rows">Row collection.</param>
        /// <param name="format">Target format.</param>
        /// <param name="fileBaseName">File name without extension; the exporter appends the extension.</param>
        ExportResult Export<T>(IEnumerable<T> rows, ExportFormat format, string fileBaseName);

        /// <summary>
        /// Exports <paramref name="rows"/> using the explicitly provided <paramref name="columns"/>.
        /// </summary>
        /// <param name="rows">Row collection as non-generic objects.</param>
        /// <param name="columns">Column metadata.</param>
        /// <param name="format">Target format.</param>
        /// <param name="fileBaseName">File name without extension; the exporter appends the extension.</param>
        ExportResult Export(IEnumerable<object> rows, IReadOnlyList<ExportColumn> columns, ExportFormat format, string fileBaseName);
    }
}
