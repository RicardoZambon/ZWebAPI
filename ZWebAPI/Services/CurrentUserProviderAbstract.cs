﻿using ZDatabase.Services.Interfaces;

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
        /// Gets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        public abstract TUserKey? UserID { get; }

        protected abstract TUserKey DefaultServiceUserID { get; }
        #endregion

        #region Constructor
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