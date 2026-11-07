using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.Factories
{
    /// <summary>
    /// Builds the projections used by the list extension tests.
    /// </summary>
    internal static class ProjectionFakeFactory
    {
        /// <summary>
        /// Projects the entities into their list model.
        /// </summary>
        /// <param name="query">The entity query.</param>
        /// <returns>The projected query.</returns>
        internal static IQueryable<EntityListModelFake> ToEntityListModel(IQueryable<EntityFake> query)
        {
            return query.Select(x => new EntityListModelFake { ID = x.ID, Name = x.Name });
        }
    }
}
