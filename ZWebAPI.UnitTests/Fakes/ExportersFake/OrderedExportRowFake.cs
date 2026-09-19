using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Row model mixing explicitly ordered and unordered properties.
    /// </summary>
    internal class OrderedExportRowFake
    {
        /// <summary>Gets or sets the value rendered in the second column.</summary>
        [ExportColumn(Order = 2)]
        public string? Second { get; set; }

        /// <summary>Gets or sets the value rendered in the first column.</summary>
        [ExportColumn(Order = 1)]
        public string? First { get; set; }

        /// <summary>Gets or sets the first of the unordered values, keeping its declaration order.</summary>
        public string? Third { get; set; }

        /// <summary>Gets or sets the second of the unordered values, keeping its declaration order.</summary>
        public string? Fourth { get; set; }
    }
}
