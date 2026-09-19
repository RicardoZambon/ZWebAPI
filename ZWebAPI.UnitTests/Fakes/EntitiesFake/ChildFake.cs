namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Nested entity used to exercise composed member expressions (<c>x =&gt; x.Child.Name</c>).
    /// </summary>
    internal class ChildFake
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }
}
