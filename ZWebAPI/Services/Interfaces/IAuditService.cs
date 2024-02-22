using Microsoft.EntityFrameworkCore.ChangeTracking;
using ZDatabase.Entities;
using ZWebAPI.Interfaces;
using ZWebAPI.Models.Audit.OperationHistory;
using ZWebAPI.Models.Audit.ServiceHistory;

namespace ZWebAPI.Services.Interfaces
{
    /// <summary>
    /// Service to handle audit services and operations.
    /// </summary>
    /// <typeparam name="TUsers">The type of the users.</typeparam>
    /// <typeparam name="TUsersKey">The type of the users key.</typeparam>
    public interface IAuditService<TUsers, TUsersKey>
        where TUsers : class
        where TUsersKey : struct
    {
        /// <summary>
        /// Adds the operation history asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityEntry">The entity entry.</param>
        Task AddOperationHistoryAsync<TEntity>(EntityEntry<TEntity> entityEntry)
            where TEntity : AuditableEntity<TUsers, TUsersKey>;

        /// <summary>
        /// Begins a new service history asynchronous.
        /// </summary>
        Task BeginNewServiceHistoryAsync();

        /// <summary>
        /// Lists the operations history from the service history identifier.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="serviceHistoryID">The service history identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Query with all operations history from the service history identifier.</returns>
        Task<IQueryable<OperationsHistoryListModel>> ListEntityOperationsHistoryAsync<TEntity>(long entityID, long serviceHistoryID, IListParameters parameters)
            where TEntity : AuditableEntity<TUsers, TUsersKey>;

        /// <summary>
        /// Lists the services history from an entity identifier asynchronous.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="entityID">The entity identifier.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns>Query with all services history from the entity identifier.</returns>
        Task<IQueryable<ServicesHistoryListModel>> ListEntityServicesHistoryAsync<TEntity>(long entityID, IListParameters parameters)
            where TEntity : AuditableEntity<TUsers, TUsersKey>;
    }
}