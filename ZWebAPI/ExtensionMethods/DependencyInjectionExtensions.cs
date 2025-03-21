﻿using Microsoft.Extensions.DependencyInjection;
using ZDatabase.Entities.Audit;
using ZWebAPI.Services;
using ZWebAPI.Services.Interfaces;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="Microsoft.Extensions.DependencyInjection.IServiceCollection"/>.
    /// </summary>
    public static class DependencyInjectionExtensions
    {
        /// <summary>
        /// Adds the audit handler service to the service collection.
        /// </summary>
        /// <typeparam name="TServicesHistory">The type of the services history.</typeparam>
        /// <typeparam name="TOperationsHistory">The type of the operations history.</typeparam>
        /// <typeparam name="TUsers">The type of the users.</typeparam>
        /// <typeparam name="TUsersKey">The type of the users key.</typeparam>
        /// <param name="services">The services.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddAuditService<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>(this IServiceCollection services)
            where TServicesHistory : ServicesHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>, new()
            where TOperationsHistory : OperationsHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>, new()
            where TUsers : class
            where TUsersKey : struct
            => services
                .AddScoped<IAuditService<TUsers, TUsersKey>, AuditServiceDefault<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>>();
    }
}