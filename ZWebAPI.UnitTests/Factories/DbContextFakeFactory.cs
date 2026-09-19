using Microsoft.EntityFrameworkCore;
using ZWebAPI.UnitTests.Fakes;

namespace ZWebAPI.UnitTests.Factories
{
    /// <summary>
    /// Builds isolated in-memory <see cref="DbContextFake"/> instances.
    /// </summary>
    internal static class DbContextFakeFactory
    {
        /// <summary>
        /// Creates a context backed by a database unique to this call.
        /// </summary>
        /// <returns>The context instance.</returns>
        internal static DbContextFake Create()
        {
            DbContextOptions<DbContextFake> options = new DbContextOptionsBuilder<DbContextFake>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .EnableSensitiveDataLogging()
                .Options;

            return new DbContextFake(options);
        }
    }
}
