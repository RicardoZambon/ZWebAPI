using System;
using System.Globalization;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Formats column values into their exported string representation for text-based formats.
    /// Type-aware exporters (Excel) may bypass this and write native types directly.
    /// </summary>
    public static class ExportValueFormatter
    {
        #region Variables
        /// <summary>
        /// Culture used for all formatting. Uses pt-BR because the application targets Brazilian
        /// users — decimals as <c>1.234,56</c> and dates as <c>dd/MM/yyyy</c>.
        /// </summary>
        public static readonly CultureInfo Culture = new("pt-BR");
        #endregion

        #region Public methods
        /// <summary>
        /// Converts <paramref name="value"/> into its exported string representation.
        /// </summary>
        public static string Format(object? value, ExportColumnType type)
        {
            if (value is null)
            {
                return string.Empty;
            }

            return type switch
            {
                ExportColumnType.Date => value is DateTime dt ? dt.ToString("dd/MM/yyyy", Culture) : value.ToString() ?? string.Empty,
                ExportColumnType.DateTime => value is DateTime dtm ? dtm.ToString("dd/MM/yyyy HH:mm", Culture) : value.ToString() ?? string.Empty,
                ExportColumnType.Currency => value is IFormattable fmt1 ? fmt1.ToString("C2", Culture) : value.ToString() ?? string.Empty,
                ExportColumnType.Number => value is IFormattable fmt2 ? fmt2.ToString("N2", Culture) : value.ToString() ?? string.Empty,
                ExportColumnType.Boolean => value is bool b ? (b ? "Sim" : "Não") : value.ToString() ?? string.Empty,
                _ => value.ToString() ?? string.Empty,
            };
        }
        #endregion
    }
}
