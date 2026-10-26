using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ZSecurity.Exceptions;
using ZWebAPI.ExceptionHandling;
using ZWebAPI.Interfaces;

namespace ZWebAPI.UnitTests.ExceptionHandling
{
    /// <summary>
    /// Unit tests for <see cref="GlobalExceptionHandler"/>.
    /// </summary>
    public class GlobalExceptionHandlerTests
    {
        /// <summary>
        /// Test the TryHandleAsync should answer an unmapped exception with a 500 problem that carries the error
        /// and trace identifiers, and no exception detail.
        /// </summary>
        [Fact]
        public async Task TryHandleAsync_Pass_AnswersUnmappedExceptionWith500AndErrorId()
        {
            // Arrange
            Guid errorId = Guid.NewGuid();
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            reporter.CaptureException(Arg.Any<Exception>()).Returns(errorId);
            HandlerHarness harness = new(reporter, isDevelopment: true);

            // Act
            bool isHandled = await harness.HandleAsync(new InvalidOperationException("boom"));

            // Assert
            isHandled.Should().BeTrue();
            harness.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            JsonElement body = harness.ReadBody();
            body.GetProperty(GlobalExceptionHandler.ErrorIdExtensionKey).GetGuid().Should().Be(errorId);
            body.TryGetProperty(GlobalExceptionHandler.TraceIdExtensionKey, out _).Should().BeTrue();
            body.TryGetProperty(GlobalExceptionHandler.ExceptionExtensionKey, out _).Should().BeFalse();
            reporter.Received(1).CaptureException(Arg.Any<InvalidOperationException>());
        }

        /// <summary>
        /// Test the TryHandleAsync should include the exception detail in development when asked to.
        /// </summary>
        [Fact]
        public async Task TryHandleAsync_Pass_IncludesExceptionDetailInDevelopmentWhenEnabled()
        {
            // Arrange
            HandlerHarness harness = new(null, isDevelopment: true, options => options.IsExceptionDetailIncludedInDevelopment = true);

            // Act
            await harness.HandleAsync(new InvalidOperationException("boom"));

            // Assert
            harness.ReadBody().GetProperty(GlobalExceptionHandler.ExceptionExtensionKey).GetProperty("message").GetString().Should().Be("boom");
        }

        /// <summary>
        /// Test the TryHandleAsync should write a mapped content body as JSON and skip the reporter for a 4xx answer.
        /// </summary>
        [Fact]
        public async Task TryHandleAsync_Pass_WritesContentBodyWithoutCapturing()
        {
            // Arrange
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            HandlerHarness harness = new(
                reporter,
                isDevelopment: false,
                options => options.Map<ArgumentException>((exception, _) => ExceptionResponse.Content(StatusCodes.Status400BadRequest, exception.Message)));

            // Act
            await harness.HandleAsync(new ArgumentException("Translation.Key"));

            // Assert
            harness.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            harness.ReadBody().GetString().Should().Be("Translation.Key");
            reporter.DidNotReceive().CaptureException(Arg.Any<Exception>());
        }

        /// <summary>
        /// Test the TryHandleAsync should still answer when the reporter itself fails.
        /// </summary>
        [Fact]
        public async Task TryHandleAsync_Pass_AnswersWhenTheReporterThrows()
        {
            // Arrange
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            reporter.CaptureException(Arg.Any<Exception>()).Returns(_ => throw new InvalidOperationException("reporter down"));
            HandlerHarness harness = new(reporter, isDevelopment: false);

            // Act
            bool isHandled = await harness.HandleAsync(new InvalidOperationException("boom"));

            // Assert
            isHandled.Should().BeTrue();
            harness.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
            harness.ReadBody().TryGetProperty(GlobalExceptionHandler.ErrorIdExtensionKey, out _).Should().BeFalse();
        }

        /// <summary>
        /// Test the TryHandleAsync should answer the default mappings registered with the options.
        /// </summary>
        [Fact]
        public async Task TryHandleAsync_Pass_AnswersDefaultMappings()
        {
            // Arrange
            HandlerHarness harness = new(null, isDevelopment: false);

            // Act
            await harness.HandleAsync(new MissingUserPermissionException("Action"));

            // Assert
            harness.HttpContext.Response.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Builds a handler over a real problem details service and an in-memory response body.
        /// </summary>
        private sealed class HandlerHarness
        {
            private readonly GlobalExceptionHandler handler;

            public HandlerHarness(IExceptionReporter? reporter, bool isDevelopment, Action<GlobalExceptionHandlerOptions>? configure = null)
            {
                ServiceCollection services = new();
                services.AddLogging();
                services.AddProblemDetails();
                if (reporter is not null)
                {
                    services.AddScoped(_ => reporter);
                }

                ServiceProvider serviceProvider = services.BuildServiceProvider();

                IHostEnvironment hostEnvironment = Substitute.For<IHostEnvironment>();
                hostEnvironment.EnvironmentName.Returns(isDevelopment ? Environments.Development : Environments.Production);

                GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions().MapDefaults();
                configure?.Invoke(options);

                handler = new GlobalExceptionHandler(
                    hostEnvironment,
                    serviceProvider.GetRequiredService<ILogger<GlobalExceptionHandler>>(),
                    Options.Create(options),
                    serviceProvider.GetRequiredService<IProblemDetailsService>());

                HttpContext = new DefaultHttpContext
                {
                    RequestServices = serviceProvider,
                };
                HttpContext.Response.Body = new MemoryStream();
            }

            public DefaultHttpContext HttpContext { get; }

            public async Task<bool> HandleAsync(Exception exception)
                => await handler.TryHandleAsync(HttpContext, exception, CancellationToken.None);

            public JsonElement ReadBody()
            {
                HttpContext.Response.Body.Position = 0;
                using JsonDocument document = JsonDocument.Parse(HttpContext.Response.Body);
                return document.RootElement.Clone();
            }
        }
    }
}
