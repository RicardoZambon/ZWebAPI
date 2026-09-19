namespace ZWebAPI.UnitTests.Fakes.ExportersFake
{
    /// <summary>
    /// Value that is neither <see cref="IFormattable"/> nor <see cref="IConvertible"/>, exercising
    /// the fallbacks taken by numeric and currency columns.
    /// </summary>
    internal class NonFormattableFake
    {
        /// <summary>The rendering returned by <see cref="ToString"/>.</summary>
        internal const string Text = "not formattable";

        /// <inheritdoc />
        public override string ToString() => Text;
    }
}
