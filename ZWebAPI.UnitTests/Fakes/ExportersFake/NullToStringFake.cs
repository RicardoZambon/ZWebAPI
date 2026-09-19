namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Value whose <see cref="object.ToString"/> returns <see langword="null"/>, exercising the
    /// null-coalescing fallbacks in the value formatter and in the Excel exporter.
    /// </summary>
    internal class NullToStringFake
    {
        /// <inheritdoc />
        public override string? ToString() => null;
    }
}
