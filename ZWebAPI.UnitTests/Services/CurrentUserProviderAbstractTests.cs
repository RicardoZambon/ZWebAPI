using ZDatabase.Services.Interfaces;
using ZWebAPI.UnitTests.Fakes.ServicesFake;

namespace ZWebAPI.UnitTests.Services
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.Services.CurrentUserProviderAbstract{TUserKey}"/>.
    /// </summary>
    public class CurrentUserProviderAbstractTests
    {
        /// <summary>
        /// Test the CurrentUserID should return the signed in user while the service-user mode is off.
        /// </summary>
        [Fact]
        public void CurrentUserID_Pass_ReturnsTheSignedInUserByDefault()
        {
            // Arrange
            ICurrentUserProvider<long> provider = new CurrentUserProviderFake { SignedInUserID = 7, ServiceUserID = 99 };

            // Act
            long? result = provider.CurrentUserID;

            // Assert
            result.Should().Be(7);
        }

        /// <summary>
        /// Test the CurrentUserID should return the default service user while the service-user mode is on.
        /// </summary>
        [Fact]
        public void CurrentUserID_Pass_ReturnsTheServiceUserWhileTheModeIsEnabled()
        {
            // Arrange
            ICurrentUserProvider<long> provider = new CurrentUserProviderFake { SignedInUserID = 7, ServiceUserID = 99 };

            // Act
            provider.EnableServiceUserMode();

            // Assert
            provider.CurrentUserID.Should().Be(99);
        }

        /// <summary>
        /// Test the CurrentUserID should return the signed in user again once the mode is disabled.
        /// </summary>
        [Fact]
        public void CurrentUserID_Pass_ReturnsTheSignedInUserAfterDisablingTheMode()
        {
            // Arrange
            ICurrentUserProvider<long> provider = new CurrentUserProviderFake { SignedInUserID = 7, ServiceUserID = 99 };
            provider.EnableServiceUserMode();

            // Act
            provider.DisableServiceUserMode();

            // Assert
            provider.CurrentUserID.Should().Be(7);
        }

        /// <summary>
        /// Test the CurrentUserID should stay empty when nobody is signed in.
        /// </summary>
        [Fact]
        public void CurrentUserID_Pass_StaysEmptyWhenNobodyIsSignedIn()
        {
            // Arrange
            ICurrentUserProvider<long> provider = new CurrentUserProviderFake { SignedInUserID = null };

            // Act
            long? result = provider.CurrentUserID;

            // Assert
            result.Should().BeNull();
        }
    }
}
