using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Records the arguments of an action as a breadcrumb when the action throws.
    /// </summary>
    /// <remarks>
    /// Controllers that leave exceptions to the global exception handler no longer have a catch block in which
    /// to add their arguments as a breadcrumb. This filter does it for them, so the captured error still says
    /// which route values, query values and body the action received.
    /// </remarks>
    /// <seealso cref="IAsyncActionFilter" />
    public sealed class ActionArgumentsBreadcrumbFilter : IAsyncActionFilter
    {
        #region Variables

        /// <summary>
        /// The description of the breadcrumb added by this filter.
        /// </summary>
        public const string BreadcrumbDescription = "Arguments of the action that threw.";
        #endregion

        #region Public methods

        /// <inheritdoc />
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(next);

            ActionExecutedContext executedContext = await next();

            if (executedContext.Exception is null || executedContext.ExceptionHandled)
            {
                return;
            }

            if (context.HttpContext.RequestServices.GetService<IExceptionReporter>() is not IExceptionReporter exceptionReporter)
            {
                return;
            }

            exceptionReporter.AddBreadcrumb(
                BreadcrumbDescription,
                new Dictionary<string, object?>(context.ActionArguments));
        }
        #endregion
    }
}
