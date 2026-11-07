using System.Linq.Expressions;

namespace ZWebAPI.Services
{
    /// <summary>
    /// Options for <see cref="AuditServiceDefault{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/>.
    /// </summary>
    /// <typeparam name="TUsers">The type of the users.</typeparam>
    public class AuditServiceOptions<TUsers>
        where TUsers : class
    {
        #region Properties
        /// <summary>
        /// Gets or sets the expression that reads a user's display name.
        /// </summary>
        /// <remarks>
        /// ZWebAPI does not know the application's user type, so it cannot tell which field holds the
        /// name shown as <see cref="Models.Audit.ServiceHistory.ServicesHistoryListModel.ChangedByName"/>.
        /// It is an expression so the database query reads it, for example <c>user => user.Name</c>.
        /// When it is <c>null</c>, <c>ChangedByName</c> stays empty.
        /// </remarks>
        /// <value>
        /// The user name selector.
        /// </value>
        public Expression<Func<TUsers, string?>>? UserNameSelector { get; set; }
        #endregion
    }
}
