namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Plain entity used by the queryable extension tests.
    /// </summary>
    internal class EntityFake
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the age.</summary>
        public int Age { get; set; }

        /// <summary>Gets or sets the child.</summary>
        public ChildFake? Child { get; set; }

        /// <summary>Gets or sets the child identifier.</summary>
        public long? ChildID { get; set; }

        /// <summary>Gets or sets the created on.</summary>
        public DateTime CreatedOn { get; set; }

        /// <summary>Gets or sets a value indicating whether the entity is active.</summary>
        public bool IsActive { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }

        /// <summary>Gets or sets the optional parent identifier.</summary>
        public long? ParentID { get; set; }

        /// <summary>Gets or sets the price.</summary>
        public decimal Price { get; set; }

        /// <summary>Gets or sets the status.</summary>
        public StatusFake Status { get; set; }
    }
}
