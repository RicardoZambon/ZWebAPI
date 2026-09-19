namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Users entity used as the <c>TUsers</c> generic argument of the audit entities.
    /// </summary>
    internal class UsersFake
    {
        /// <summary>Gets or sets the identifier.</summary>
        public long ID { get; set; }

        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }
}
