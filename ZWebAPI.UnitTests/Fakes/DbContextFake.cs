using Microsoft.EntityFrameworkCore;
using ZDatabase.Interfaces;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.Fakes
{
    /// <summary>
    /// In-memory <see cref="DbContext"/> implementing <see cref="IDbContext"/> for the tests that
    /// need a real EF Core query provider (async operators, <c>EF.Functions.Like</c>, change tracking).
    /// </summary>
    internal class DbContextFake : DbContext, IDbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextFake"/> class.
        /// </summary>
        /// <param name="options">The context options.</param>
        public DbContextFake(DbContextOptions<DbContextFake> options)
            : base(options)
        {
        }

        /// <summary>Gets the auditable entities.</summary>
        public DbSet<AuditableEntityFake> AuditableEntities => Set<AuditableEntityFake>();

        /// <summary>Gets the children.</summary>
        public DbSet<ChildFake> Children => Set<ChildFake>();

        /// <summary>Gets the entities.</summary>
        public DbSet<EntityFake> Entities => Set<EntityFake>();

        /// <summary>Gets the non-entities.</summary>
        public DbSet<NonEntityFake> NonEntities => Set<NonEntityFake>();

        /// <summary>Gets the operations history.</summary>
        public DbSet<OperationsHistoryFake> OperationsHistory => Set<OperationsHistoryFake>();

        /// <summary>Gets the services history.</summary>
        public DbSet<ServicesHistoryFake> ServicesHistory => Set<ServicesHistoryFake>();

        /// <summary>Gets the users.</summary>
        public DbSet<UsersFake> Users => Set<UsersFake>();

        /// <inheritdoc />
        public void ClearAuditServiceHistory()
        {
        }

        /// <inheritdoc />
        public TEntity CreateProxy<TEntity>(Action<TEntity>? configureEntity, params object[] constructorArguments)
            where TEntity : class
        {
            throw new NotSupportedException("Proxies are not supported by the in-memory test context.");
        }

        /// <inheritdoc />
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AuditableEntityFake>().Ignore(x => x.RowVersion);
            modelBuilder.Entity<AuditableEntityFake>().HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedByID);
            modelBuilder.Entity<AuditableEntityFake>().HasOne(x => x.LastChangedBy).WithMany().HasForeignKey(x => x.LastChangedByID);

            modelBuilder.Entity<ServicesHistoryFake>().HasOne(x => x.ChangedBy).WithMany().HasForeignKey(x => x.ChangedByID);
            modelBuilder.Entity<OperationsHistoryFake>().HasOne(x => x.ServiceHistory).WithMany(x => x.Operations).HasForeignKey(x => x.ServiceHistoryID);
        }
    }
}
