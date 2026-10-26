using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using ZDatabase.Interfaces;

namespace ZWebAPI.HealthChecks
{
    /// <summary>
    /// Reports whether the configured database answers, for the readiness probe.
    /// </summary>
    /// <seealso cref="IHealthCheck" />
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        #region Variables
        private readonly IDbContext dbContext;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseHealthCheck"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="IDbContext"/> instance.</param>
        public DatabaseHealthCheck(IDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        #endregion

        #region Public methods

        /// <summary>
        /// Checks whether the database accepts a connection.
        /// </summary>
        /// <param name="context">The <see cref="HealthCheckContext"/> instance.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Healthy when the database answers, unhealthy otherwise.</returns>
        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            if (dbContext is not DbContext entityFrameworkContext)
            {
                return HealthCheckResult.Unhealthy("The registered IDbContext is not an Entity Framework DbContext.");
            }

            try
            {
                return await entityFrameworkContext.Database.CanConnectAsync(cancellationToken)
                    ? HealthCheckResult.Healthy("The database answered.")
                    : HealthCheckResult.Unhealthy("The database refused the connection.");
            }
            catch (Exception ex)
            {
                // Answered rather than rethrown: an exception escaping here is a 500 with no body,
                // which tells the orchestrator nothing about whether the process or the database is
                // the part that is down. No breadcrumb either - a probe runs on a timer, and a
                // database outage would otherwise report one event per poll.
                return HealthCheckResult.Unhealthy("The database connection attempt failed.", ex);
            }
        }
        #endregion
    }
}
