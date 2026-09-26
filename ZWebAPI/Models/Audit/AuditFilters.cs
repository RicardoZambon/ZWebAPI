namespace ZWebAPI.Models.Audit
{
    /// <summary>
    /// Names of the filters <see cref="ZWebAPI.Services.Interfaces.IAuditService{TUsers, TUsersKey}"/>
    /// understands in an <see cref="ZWebAPI.Interfaces.IListParameters"/>.
    /// </summary>
    /// <remarks>
    /// A filter arrives as a dictionary key, so the client and the server agree on a string and
    /// nothing checks that they still do. These constants are that string, named once, so a
    /// consumer can reference them instead of spelling them again.
    /// Lookups are case insensitive, which is what lets a camelCase form control reach a
    /// PascalCase property.
    /// </remarks>
    public static class AuditFilters
    {
        /// <summary>
        /// Identifier of the user who ran the service. Matched exactly.
        /// </summary>
        public const string ChangedByID = nameof(ChangedByID);

        /// <summary>
        /// Earliest instant to include, compared against the service history's ChangedOn and
        /// inclusive of it. UTC, because that is how the instant is stored.
        /// </summary>
        public const string ChangedOnFrom = nameof(ChangedOnFrom);

        /// <summary>
        /// Latest instant to include, compared against the service history's ChangedOn and
        /// inclusive of it. UTC, because that is how the instant is stored.
        /// </summary>
        public const string ChangedOnTo = nameof(ChangedOnTo);

        /// <summary>
        /// Name of the service that ran, in its <c>IService\Method</c> form. Matched with LIKE.
        /// </summary>
        public const string Name = nameof(Name);

        /// <summary>
        /// Restricts the operations listed for a service history to the record being audited.
        /// </summary>
        /// <remarks>
        /// A service usually writes to more than one table, and by default every operation it
        /// performed is listed -- which is what shows the related rows a change also touched.
        /// Set this to narrow the list to the audited record alone. It has no effect on the
        /// services list, which is already restricted to the services that touched the record.
        /// </remarks>
        public const string OnlyCurrentEntity = nameof(OnlyCurrentEntity);
    }
}
