using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ZDatabase.Entities;
using ZDatabase.Entities.Audit;
using ZDatabase.Entries;
using ZDatabase.ExtensionMethods;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="ZDatabase.Entries.AuditEntry"/>.
    /// </summary>
    internal static class AuditEntryExtensions
    {
        /// <summary>
        /// Generates the operations history.
        /// </summary>
        /// <typeparam name="TServicesHistory">The type of the services history.</typeparam>
        /// <typeparam name="TOperationsHistory">The type of the operations history.</typeparam>
        /// <typeparam name="TUsers">The type of the users.</typeparam>
        /// <typeparam name="TUsersKey">The type of the users key.</typeparam>
        /// <param name="entry">The entry.</param>
        /// <param name="servicesHistory">The services history.</param>
        /// <returns>The operation history instance.</returns>
        internal static TOperationsHistory GenerateOperationsHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>(this AuditEntry entry, TServicesHistory servicesHistory)
            where TServicesHistory : ServicesHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>
            where TOperationsHistory : OperationsHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>, new()
            where TUsers : class
            where TUsersKey : struct
        {
            return new TOperationsHistory
            {
                ServiceHistoryID = servicesHistory.ID,
                ServiceHistory = servicesHistory,

                TableName = entry.Entry.Metadata.GetTableName(),
                EntityName = entry.Entry.Metadata.DisplayName(),
                EntityID = entry.Entry.Entity.GetType().IsSubclassOf(typeof(Entity)) ? (long)(entry.Entry.Property(nameof(Entity.ID)).CurrentValue ?? 0) : null,

                OperationType = entry.OriginalState.ToString(),
                OldValues = JsonSerializer.Serialize(entry.GetOldValues()),
                NewValues = JsonSerializer.Serialize(entry.GetNewValues())
            };
        }
    }
}