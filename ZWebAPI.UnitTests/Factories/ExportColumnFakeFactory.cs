using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Factories
{
    /// <summary>
    /// Builds hand-written <see cref="ExportColumn"/> lists so the exporters can be driven with
    /// arbitrary raw values, including values whose CLR type does not match the column type.
    /// </summary>
    internal static class ExportColumnFakeFactory
    {
        /// <summary>
        /// Creates a single column whose selector returns the row itself.
        /// </summary>
        /// <param name="type">The column type.</param>
        /// <param name="header">The column header.</param>
        /// <returns>The column list.</returns>
        internal static IReadOnlyList<ExportColumn> Identity(ExportColumnType type, string header = "Value")
        {
            return new List<ExportColumn>
            {
                new(header, type, row => row is NullRow ? null : row),
            };
        }

        /// <summary>
        /// Creates a row placeholder whose selector resolves to <see langword="null"/>.
        /// </summary>
        /// <returns>The placeholder instance.</returns>
        internal static object NullValueRow() => new NullRow();

        private sealed class NullRow
        {
        }
    }
}
