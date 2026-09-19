using AutoMapper;
using ZWebAPI.Models.Audit.OperationHistory;
using ZWebAPI.Models.Audit.ServiceHistory;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.Factories
{
    /// <summary>
    /// Builds the AutoMapper configuration used by the audit service and the list extension tests.
    /// </summary>
    internal static class MapperFakeFactory
    {
        /// <summary>
        /// Creates a mapper wired with the audit list-model projections.
        /// </summary>
        /// <returns>The mapper instance.</returns>
        internal static IMapper Create()
        {
            return CreateConfiguration().CreateMapper();
        }

        /// <summary>
        /// Creates the configuration provider wired with the audit list-model projections.
        /// </summary>
        /// <returns>The configuration provider instance.</returns>
        internal static IConfigurationProvider CreateConfiguration()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<ServicesHistoryFake, ServicesHistoryListModel>()
                    .ForMember(x => x.ChangedByName, o => o.MapFrom(x => x.ChangedBy!.Name));

                cfg.CreateMap<OperationsHistoryFake, OperationsHistoryListModel>();

                cfg.CreateMap<EntityFake, EntityListModelFake>();
            });
        }
    }
}
