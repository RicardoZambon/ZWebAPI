using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Row model whose column attribute carries a blank header, which must fall back to the property name.
    /// </summary>
    internal class BlankHeaderExportRowFake
    {
        /// <summary>Gets or sets the value annotated with a whitespace-only header.</summary>
        [ExportColumn("   ")]
        public string? Blank { get; set; }
    }
}
