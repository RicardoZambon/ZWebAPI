namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Supported list export formats.
    /// </summary>
    public enum ExportFormat
    {
        /// <summary>Excel workbook (.xlsx).</summary>
        Xlsx = 0,

        /// <summary>Comma-separated values (.csv).</summary>
        Csv = 1,

        /// <summary>Portable Document Format (.pdf).</summary>
        Pdf = 2,

        /// <summary>XML document (.xml).</summary>
        Xml = 3,

        /// <summary>Web archive (.mhtml).</summary>
        Mhtml = 4,
    }
}
