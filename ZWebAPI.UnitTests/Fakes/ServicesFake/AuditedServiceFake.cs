using System.Runtime.CompilerServices;
using ZSecurity.Attributes;
using ZWebAPI.Services.Interfaces;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.Fakes.ServicesFake
{
    /// <summary>
    /// Contract implemented by <see cref="AuditedServiceFake"/>; its name is what the audit service
    /// records as the service portion of the service-history name.
    /// </summary>
    internal interface IAuditedServiceFake
    {
        /// <summary>Starts a new service history.</summary>
        /// <returns>The running task.</returns>
        Task BeginAsync();
    }

    /// <summary>
    /// Service whose method carries <see cref="ActionMethodAttribute"/>, so the audit service can
    /// resolve it from the stack trace.
    /// </summary>
    internal class AuditedServiceFake : IAuditedServiceFake
    {
        private readonly IAuditService<UsersFake, long> auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditedServiceFake"/> class.
        /// </summary>
        /// <param name="auditService">The audit service under test.</param>
        internal AuditedServiceFake(IAuditService<UsersFake, long> auditService)
        {
            this.auditService = auditService;
        }

        /// <inheritdoc />
        [ActionMethod]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public Task BeginAsync()
        {
            return auditService.BeginNewServiceHistoryAsync();
        }
    }

    /// <summary>
    /// Service whose method does NOT carry <see cref="ActionMethodAttribute"/>.
    /// </summary>
    internal class UnauditedServiceFake
    {
        private readonly IAuditService<UsersFake, long> auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnauditedServiceFake"/> class.
        /// </summary>
        /// <param name="auditService">The audit service under test.</param>
        internal UnauditedServiceFake(IAuditService<UsersFake, long> auditService)
        {
            this.auditService = auditService;
        }

        /// <summary>Starts a new service history from a method the helper cannot resolve.</summary>
        /// <returns>The running task.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal Task BeginAsync()
        {
            return auditService.BeginNewServiceHistoryAsync();
        }
    }
}
