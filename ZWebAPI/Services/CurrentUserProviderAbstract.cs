using ZDatabase.Services.Interfaces;

namespace ZWebAPI.Services
{
    /// <inheritdoc />
    public abstract class CurrentUserProviderAbstract<TUserKey> : ICurrentUserProvider<TUserKey>
        where TUserKey : struct
    {
        #region Variables
        private bool serviceUserMode = false;
        #endregion

        #region Properties
        /// <inheritdoc />
        public TUserKey? CurrentUserID { get => serviceUserMode ? DefaultServiceUserID : UserID; }

        /// <summary>
        /// Gets the default service user identifier.
        /// </summary>
        /// <value>
        /// The default service user identifier.
        /// </value>
        protected abstract TUserKey DefaultServiceUserID { get; }

        /// <summary>
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public abstract TUserKey? UserID { get; }
        #endregion

        #region Constructors
        #endregion

        #region Public methods
        /// <inheritdoc />
        public void DisableServiceUserMode()
        {
            serviceUserMode = false;
        }

        /// <inheritdoc />
        public void EnableServiceUserMode()
        {
            serviceUserMode = true;
        }
        #endregion

        #region Private methods
        #endregion
    }
}