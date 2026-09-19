namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Row model holding a write-only property, which must be skipped by the column resolver.
    /// </summary>
    internal class WriteOnlyExportRowFake
    {
        private string? writeOnly;

        /// <summary>Sets the write-only value.</summary>
        public string? WriteOnly
        {
            set => writeOnly = value;
        }

        /// <summary>Gets or sets the readable value.</summary>
        public string? Readable { get; set; }

        /// <summary>Gets the last value written through <see cref="WriteOnly"/>.</summary>
        /// <returns>The stored value.</returns>
        internal string? GetWriteOnly() => writeOnly;
    }
}
