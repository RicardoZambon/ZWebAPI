namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Destination list model used when projecting <see cref="EntityFake"/> rows.
    /// </summary>
    internal class EntityListModelFake
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }
}
