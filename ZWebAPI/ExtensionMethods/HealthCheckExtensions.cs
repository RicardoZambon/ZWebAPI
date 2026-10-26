using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ZWebAPI.HealthChecks;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods to register the liveness and readiness probes.
    /// </summary>
    public static class HealthCheckExtensions
    {
        #region Variables

        /// <summary>
        /// The tag that puts a health check in the readiness probe. A check without it is reported
        /// by neither probe, so tag every check the application adds that must gate traffic.
        /// </summary>
        public const string ReadyTag = "ready";
        #endregion

        #region Public methods

        /// <summary>
        /// Adds the database readiness check.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <returns>
        /// The health checks builder, so the application can chain its own checks. Tag any of them
        /// with <see cref="ReadyTag"/> to have <see cref="MapDefaultHealthChecks"/> report them.
        /// </returns>
        /// <remarks>
        /// The check resolves <c>IDbContext</c> per request rather than holding one, so an
        /// application that registers its context as scoped is probed with a fresh connection.
        /// </remarks>
        public static IHealthChecksBuilder AddDefaultHealthChecks(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            return services
                .AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("database", tags: [ReadyTag]);
        }

        /// <summary>
        /// Maps the liveness and readiness probes.
        /// </summary>
        /// <param name="endpoints">The endpoint route builder.</param>
        /// <param name="livenessPath">The liveness path. Defaults to <c>/health</c>.</param>
        /// <param name="readinessPath">The readiness path. Defaults to <c>/health/ready</c>.</param>
        /// <returns>The endpoint route builder.</returns>
        /// <remarks>
        /// Two probes, because the answers are acted on differently. Liveness says the process is up
        /// and runs no check at all: restarting an instance does not fix an unreachable database, it
        /// just removes the one that could have served the request the moment the database returns.
        /// Readiness says the instance can actually serve, and reports every check tagged
        /// <see cref="ReadyTag"/>.
        /// <para>
        /// Both probes answer anonymously. They carry no detail beyond healthy or unhealthy, but an
        /// application that exposes them to the internet is still telling a stranger whether its
        /// database is down -- map them on an internal port or behind the gateway where that matters.
        /// </para>
        /// </remarks>
        public static IEndpointRouteBuilder MapDefaultHealthChecks(this IEndpointRouteBuilder endpoints, string livenessPath = "/health", string readinessPath = "/health/ready")
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            endpoints.MapHealthChecks(livenessPath, new HealthCheckOptions { Predicate = _ => false });
            endpoints.MapHealthChecks(readinessPath, new HealthCheckOptions { Predicate = healthCheck => healthCheck.Tags.Contains(ReadyTag) });

            return endpoints;
        }
        #endregion
    }
}
