using ZWebAPI.Services;

namespace ZWebAPI.UnitTests.Fakes.ServicesFake
{
    /// <summary>
    /// Concrete <see cref="CurrentUserProviderAbstract{TUserKey}"/> used to exercise the service-user mode.
    /// </summary>
    internal class CurrentUserProviderFake : CurrentUserProviderAbstract<long>
    {
        /// <summary>Gets or sets the identifier returned while the service-user mode is enabled.</summary>
        internal long ServiceUserID { get; set; } = 99;

        /// <summary>Gets or sets the identifier returned while the service-user mode is disabled.</summary>
        internal long? SignedInUserID { get; set; } = 1;

        /// <inheritdoc />
        protected override long DefaultServiceUserID => ServiceUserID;

        /// <inheritdoc />
        public override long? UserID => SignedInUserID;
    }
}
