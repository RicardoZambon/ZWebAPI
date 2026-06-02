using System;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Metadata for a single column in an exported list.
    /// </summary>
    public sealed class ExportColumn
    {
        #region Properties
        /// <summary>
        /// Column header text displayed in the exported file.
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// Format type for type-aware exporters (Excel, PDF).
        /// </summary>
        public ExportColumnType Type { get; }

        /// <summary>
        /// Selector extracting the column value from a row instance.
        /// </summary>
        public Func<object, object?> ValueSelector { get; }
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportColumn"/> class.
        /// </summary>
        /// <param name="header">Column header text (already translated if applicable).</param>
        /// <param name="type">Format type for type-aware exporters.</param>
        /// <param name="valueSelector">Selector extracting the column value from a row instance.</param>
        public ExportColumn(string header, ExportColumnType type, Func<object, object?> valueSelector)
        {
            Header = header;
            Type = type;
            ValueSelector = valueSelector;
        }
        #endregion
    }
}
