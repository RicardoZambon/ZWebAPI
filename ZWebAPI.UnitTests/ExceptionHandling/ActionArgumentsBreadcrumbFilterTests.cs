using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using ZWebAPI.ExceptionHandling;
using ZWebAPI.Interfaces;

namespace ZWebAPI.UnitTests.ExceptionHandling
{
    /// <summary>
    /// Unit tests for <see cref="ActionArgumentsBreadcrumbFilter"/>.
    /// </summary>
    public class ActionArgumentsBreadcrumbFilterTests
    {
        /// <summary>
        /// Test the OnActionExecutionAsync should add the action arguments as a breadcrumb when the action throws.
        /// </summary>
        [Fact]
        public async Task OnActionExecutionAsync_Pass_AddsArgumentsWhenTheActionThrows()
        {
            // Arrange
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            ActionExecutingContext context = CreateContext(reporter);

            // Act
            await new ActionArgumentsBreadcrumbFilter().OnActionExecutionAsync(context, () => Executed(context, new InvalidOperationException(), false));

            // Assert
            reporter.Received(1).AddBreadcrumb(
                ActionArgumentsBreadcrumbFilter.BreadcrumbDescription,
                Arg.Is<IDictionary<string, object?>>(arguments => (int)arguments["id"]! == 5));
        }

        /// <summary>
        /// Test the OnActionExecutionAsync should add nothing when the action succeeds.
        /// </summary>
        [Fact]
        public async Task OnActionExecutionAsync_Pass_AddsNothingWhenTheActionSucceeds()
        {
            // Arrange
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            ActionExecutingContext context = CreateContext(reporter);

            // Act
            await new ActionArgumentsBreadcrumbFilter().OnActionExecutionAsync(context, () => Executed(context, null, false));

            // Assert
            reporter.DidNotReceiveWithAnyArgs().AddBreadcrumb(default!, default!);
        }

        /// <summary>
        /// Test the OnActionExecutionAsync should add nothing when another filter already handled the exception.
        /// </summary>
        [Fact]
        public async Task OnActionExecutionAsync_Pass_AddsNothingWhenTheExceptionWasHandled()
        {
            // Arrange
            IExceptionReporter reporter = Substitute.For<IExceptionReporter>();
            ActionExecutingContext context = CreateContext(reporter);

            // Act
            await new ActionArgumentsBreadcrumbFilter().OnActionExecutionAsync(context, () => Executed(context, new InvalidOperationException(), true));

            // Assert
            reporter.DidNotReceiveWithAnyArgs().AddBreadcrumb(default!, default!);
        }

        private static ActionExecutingContext CreateContext(IExceptionReporter reporter)
        {
            ServiceCollection services = new();
            services.AddScoped(_ => reporter);

            DefaultHttpContext httpContext = new()
            {
                RequestServices = services.BuildServiceProvider(),
            };

            ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());

            return new ActionExecutingContext(
                actionContext,
                [],
                new Dictionary<string, object?> { { "id", 5 } },
                new object());
        }

        private static Task<ActionExecutedContext> Executed(ActionExecutingContext context, Exception? exception, bool isHandled)
        {
            ActionExecutedContext executedContext = new(context, [], context.Controller)
            {
                Exception = exception,
                ExceptionHandled = isHandled,
            };

            return Task.FromResult(executedContext);
        }
    }
}
