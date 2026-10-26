using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using ZDatabase.Interfaces;
using ZWebAPI.ExceptionHandling;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.HealthChecks;
using ZWebAPI.UnitTests.Factories;
using ZWebAPI.UnitTests.Fakes;

namespace ZWebAPI.UnitTests.HealthChecks
{
    /// <summary>
    /// Unit tests for <see cref="DatabaseHealthCheck"/> and the registration extensions.
    /// </summary>
    public class DatabaseHealthCheckTests
    {
        /// <summary>
        /// Test the CheckHealthAsync should report healthy when the database answers.
        /// </summary>
        [Fact]
        public async Task CheckHealthAsync_Pass_ReportsHealthyWhenTheDatabaseAnswers()
        {
            // Arrange
            using DbContextFake context = DbContextFakeFactory.Create();

            // Act
            HealthCheckResult result = await new DatabaseHealthCheck(context).CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Healthy);
        }

        /// <summary>
        /// Test the CheckHealthAsync should report unhealthy when the context is not an EF Core context.
        /// </summary>
        [Fact]
        public async Task CheckHealthAsync_Fail_ReportsUnhealthyForANonEntityFrameworkContext()
        {
            // Arrange
            IDbContext context = Substitute.For<IDbContext>();

            // Act
            HealthCheckResult result = await new DatabaseHealthCheck(context).CheckHealthAsync(new HealthCheckContext());

            // Assert
            result.Status.Should().Be(HealthStatus.Unhealthy);
        }

        /// <summary>
        /// Test the AddDefaultHealthChecks should register the database check with the readiness tag.
        /// </summary>
        [Fact]
        public void AddDefaultHealthChecks_Pass_RegistersTheDatabaseCheckAsReady()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddDefaultHealthChecks();

            // Assert
            HealthCheckServiceOptions options = services.BuildServiceProvider().GetRequiredService<IOptions<HealthCheckServiceOptions>>().Value;
            options.Registrations.Should().ContainSingle(registration => registration.Name == "database"
                && registration.Tags.Contains(HealthCheckExtensions.ReadyTag));
        }

        /// <summary>
        /// Test the AddGlobalExceptionHandling should register the defaults, the application mappings and the
        /// breadcrumb filter.
        /// </summary>
        [Fact]
        public void AddGlobalExceptionHandling_Pass_RegistersMappingsAndBreadcrumbFilter()
        {
            // Arrange
            ServiceCollection services = new();

            // Act
            services.AddGlobalExceptionHandling(options => options.IsCaptureEnabled = false);

            // Assert
            ServiceProvider serviceProvider = services.BuildServiceProvider();
            serviceProvider.GetRequiredService<IOptions<GlobalExceptionHandlerOptions>>().Value.IsCaptureEnabled.Should().BeFalse();
            serviceProvider.GetRequiredService<IOptions<ExceptionHandlerOptions>>().Value.AllowStatusCode404Response.Should().BeTrue();
            serviceProvider.GetRequiredService<IOptions<MvcOptions>>().Value.Filters
                .OfType<TypeFilterAttribute>()
                .Should().Contain(filter => filter.ImplementationType == typeof(ActionArgumentsBreadcrumbFilter));
        }
    }
}
