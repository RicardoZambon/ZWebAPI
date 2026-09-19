using ZDatabase.Entities;

namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Auditable entity used by the audit service tests.
    /// </summary>
    internal class AuditableEntityFake : AuditableEntity<UsersFake, long>
    {
        /// <summary>Gets or sets the name.</summary>
        public string? Name { get; set; }
    }
}
