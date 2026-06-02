using System;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Optional attribute overriding default export behavior for a list-model property.
    /// By default every public readable property is exported using the property name as the header.
    /// Apply this attribute only to change the header, skip the property, or specify a format type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ExportColumnAttribute : Attribute
    {
        #region Properties
        /// <summary>
        /// Optional header/i18n key. When <c>null</c>, the property name is used.
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// When <c>true</c>, the property is skipped entirely from exports.
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// Column ordinal when multiple columns need explicit ordering. Lower values appear first.
        /// Properties without an order preserve their declaration order.
        /// </summary>
        public int Order { get; set; } = int.MaxValue;

        /// <summary>
        /// Format type for type-aware exporters (Excel, PDF). Defaults to <see cref="ExportColumnType.Text"/>.
        /// </summary>
        public ExportColumnType Type { get; set; } = ExportColumnType.Text;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExportColumnAttribute"/> class.
        /// </summary>
        public ExportColumnAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportColumnAttribute"/> class with a custom header.
        /// </summary>
        /// <param name="header">The header (or i18n key) to display instead of the property name.</param>
        public ExportColumnAttribute(string header)
        {
            Header = header;
        }
        #endregion
    }
}
