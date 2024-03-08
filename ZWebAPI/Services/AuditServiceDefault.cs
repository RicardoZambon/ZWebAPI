using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Reflection;
using ZDatabase.Entities;
using ZDatabase.Entities.Audit;
using ZDatabase.Entries;
using ZDatabase.Exceptions;
using ZDatabase.Interfaces;
using ZDatabase.Repositories.Audit.Interfaces;
using ZSecurity.Helpers;
using ZSecurity.Services;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.Interfaces;
using ZWebAPI.Models.Audit.OperationHistory;
using ZWebAPI.Models.Audit.ServiceHistory;
using ZWebAPI.Services.Interfaces;

namespace ZWebAPI.Services
{
    /// <inheritdoc />
    public class AuditServiceDefault<TServicesHistory, TOperationsHistory, TUsers, TUsersKey> : IAuditService<TUsers, TUsersKey>
        where TServicesHistory : ServicesHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>, new()
        where TOperationsHistory : OperationsHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>, new()
        where TUsers : class
        where TUsersKey : struct
    {
        #region Variables
        private readonly IDbContext dbContext;
        private readonly IMapper mapper;
        private readonly IOperationsHistoryRepository<TServicesHistory, TOperationsHistory, TUsers, TUsersKey> operationsHistoryRepository;
        private readonly ISecurityHandler securityHandler;
        private readonly IServicesHistoryRepository<TServicesHistory, TOperationsHistory, TUsers, TUsersKey> servicesHistoryRepository;
        #endregion

        #region Properties
        protected TServicesHistory? CurrentServiceHistory { get; private set; } = null;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AuditServiceDefault{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/> class.
        /// </summary>
        /// <param name="dbContext">The <see cref="IDbContext"/> instance.</param>
        /// <param name="mapper">The <see cref="AutoMapper.IMapper"/> instance.</param>
        /// <param name="operationsHistoryRepository">The <see cref="ZDatabase.Repositories.Audit.Interfaces.IOperationsHistoryRepository{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/> instance.</param>
        /// <param name="securityHandler">The <see cref="ZDatabase.Services.Interfaces.IAuditHandler"/> instance.</param>
        /// <param name="servicesHistoryRepository">The <see cref="ZDatabase.Repositories.Audit.Interfaces.IServicesHistoryRepository{TServicesHistory, TOperationsHistory, TUsers, TUsersKey}"/> instance.</param>
        public AuditServiceDefault(
            IDbContext dbContext,
            IMapper mapper,
            IOperationsHistoryRepository<TServicesHistory, TOperationsHistory, TUsers, TUsersKey> operationsHistoryRepository,
            ISecurityHandler securityHandler,
            IServicesHistoryRepository<TServicesHistory, TOperationsHistory, TUsers, TUsersKey> servicesHistoryRepository)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.operationsHistoryRepository = operationsHistoryRepository;
            this.securityHandler = securityHandler;
            this.servicesHistoryRepository = servicesHistoryRepository;
        }
        #endregion

        #region Public methods
        /// <inheritdoc />
        public async Task AddOperationHistoryAsync<TEntity>(EntityEntry<TEntity> entityEntry)
            where TEntity : AuditableEntity<TUsers, TUsersKey>
        {
            if (CurrentServiceHistory is null)
            {
                throw new MissingServiceHistoryException();
            }

            AuditEntry auditTabelasValores = new(entityEntry);

            TOperationsHistory historyOperation = auditTabelasValores.GenerateOperationsHistory<TServicesHistory, TOperationsHistory, TUsers, TUsersKey>(CurrentServiceHistory);
            await operationsHistoryRepository.AddOperationHistoryAsync(historyOperation);
        }

        /// <inheritdoc />
        public async Task BeginNewServiceHistoryAsync()
        {
            MethodBase method = StackTraceHelper.GetMethodImplementingActionMethodAttribute()
                ?? throw new Exception("Could not find any previous method in StackTrace implementing ActionMethodAttribute.");

            string? serviceName = method.DeclaringType?.GetInterfaces().FirstOrDefault()?.Name;
            string? methodName = method.Name;

            await securityHandler.ValidateUserHasPermissionAsync();

            CurrentServiceHistory = new TServicesHistory { Name = $"{serviceName}\\{methodName}" };
            await servicesHistoryRepository.AddServiceHistoryAsync(CurrentServiceHistory);
        }

        /// <inheritdoc />
        public async Task<IQueryable<ServicesHistoryListModel>> ListEntityServicesHistoryAsync<TEntity>(long entityID, IListParameters parameters)
            where TEntity : AuditableEntity<TUsers, TUsersKey>
        {
            return (await servicesHistoryRepository.ListServicesAsync<TEntity>(entityID))
                .GetRange(parameters)
                .ProjectTo<ServicesHistoryListModel>(mapper.ConfigurationProvider);
        }

        /// <inheritdoc />
        public async Task<IQueryable<OperationsHistoryListModel>> ListEntityOperationsHistoryAsync<TEntity>(long entityID, long serviceHistoryID, IListParameters parameters)
            where TEntity : AuditableEntity<TUsers, TUsersKey>
        {
            if (await dbContext.FindAsync<TEntity>(entityID) is not TEntity entity)
            {
                throw new EntityNotFoundException<TEntity>(entityID);
            }

            if (await dbContext.FindAsync<TServicesHistory>(serviceHistoryID) is not TServicesHistory serviceHistory)
            {
                throw new EntityNotFoundException<TServicesHistory>(serviceHistoryID);
            }

            IQueryable<TOperationsHistory> operations = operationsHistoryRepository.ListOperations(serviceHistoryID);

            // The returned operations must contain the entity identifier and the table name.
            EntityEntry<TEntity> entry = dbContext.Entry(entity);
            if (!await operations.AnyAsync(x => EF.Functions.Like(x.TableName ?? string.Empty, entry.Metadata.GetTableName() ?? string.Empty) && x.EntityID == entityID))
            {
                return Enumerable.Empty<OperationsHistoryListModel>().AsQueryable();
            }

            return operations
                .GetRange(parameters)
                .ProjectTo<OperationsHistoryListModel>(mapper.ConfigurationProvider);
        }
        #endregion

        #region Private methods
        #endregion
    }
}