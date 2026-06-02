namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Describes how an exported column value should be formatted.
    /// </summary>
    public enum ExportColumnType
    {
        /// <summary>Raw text.</summary>
        Text = 0,

        /// <summary>Integer or decimal number.</summary>
        Number = 1,

        /// <summary>Date (no time component).</summary>
        Date = 2,

        /// <summary>Date and time.</summary>
        DateTime = 3,

        /// <summary>Currency amount.</summary>
        Currency = 4,

        /// <summary>Boolean.</summary>
        Boolean = 5,
    }
}
