using ZDatabase.Entities.Audit;

namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Services history entity used by the audit service tests.
    /// </summary>
    internal class ServicesHistoryFake : ServicesHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>
    {
    }
}
