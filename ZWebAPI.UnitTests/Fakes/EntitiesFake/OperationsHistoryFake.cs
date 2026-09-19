using ZDatabase.Entities.Audit;

namespace ZWebAPI.UnitTests.Fakes.EntitiesFake
{
    /// <summary>
    /// Operations history entity used by the audit service tests.
    /// </summary>
    internal class OperationsHistoryFake : OperationsHistory<ServicesHistoryFake, OperationsHistoryFake, UsersFake, long>
    {
    }
}
