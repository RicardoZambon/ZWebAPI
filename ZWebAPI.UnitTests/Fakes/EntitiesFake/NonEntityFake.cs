namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Tracked type that does NOT derive from <see cref="ZDatabase.Entities.Entity"/>, used to
    /// exercise the branch where an audited entry cannot expose an entity identifier.
    /// </summary>
    internal class NonEntityFake
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the description.</summary>
        public string? Description { get; set; }
    }
}
