using ZWebAPI.Exporters;

namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Row model covering every <see cref="ExportColumnType"/> plus a custom header and an ignored property.
    /// </summary>
    internal class ExportRowFake
    {
        /// <summary>Gets or sets the identifier, exported with the property name as header.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the name, exported under a custom header.</summary>
        [ExportColumn("Full name")]
        public string? Name { get; set; }

        /// <summary>Gets or sets the amount, exported as a number.</summary>
        [ExportColumn(Type = ExportColumnType.Number)]
        public decimal Amount { get; set; }

        /// <summary>Gets or sets the price, exported as currency.</summary>
        [ExportColumn(Type = ExportColumnType.Currency)]
        public decimal Price { get; set; }

        /// <summary>Gets or sets the creation date, exported as a date.</summary>
        [ExportColumn(Type = ExportColumnType.Date)]
        public DateTime CreatedOn { get; set; }

        /// <summary>Gets or sets the change timestamp, exported as a date and time.</summary>
        [ExportColumn(Type = ExportColumnType.DateTime)]
        public DateTime ChangedOn { get; set; }

        /// <summary>Gets or sets a value indicating whether the row is active, exported as a boolean.</summary>
        [ExportColumn(Type = ExportColumnType.Boolean)]
        public bool IsActive { get; set; }

        /// <summary>Gets or sets a value that must never reach the exported file.</summary>
        [ExportColumn(Ignore = true)]
        public string? Secret { get; set; }
    }
}
