using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using ZWebAPI.ExceptionHandling;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods to register the global exception handling.
    /// </summary>
    public static class ExceptionHandlingExtensions
    {
        #region Public methods

        /// <summary>
        /// Adds the global exception handling, so controllers no longer need a <c>try/catch</c> per action.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configure">
        /// The optional configuration callback, used to register the application specific exception mappings.
        /// </param>
        /// <returns>The service collection.</returns>
        /// <remarks>
        /// The mappings from <see cref="GlobalExceptionHandlerOptions.MapDefaults"/> are registered before
        /// <paramref name="configure"/> runs, so a consuming application can override any of them.
        /// </remarks>
        public static IServiceCollection AddGlobalExceptionHandling(this IServiceCollection services, Action<GlobalExceptionHandlerOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddProblemDetails();

            // EntityNotFoundException maps to 404, and without this the exception handler middleware treats a
            // 404 written by a handler as unhandled and rethrows it.
            services.Configure<ExceptionHandlerOptions>(options => options.AllowStatusCode404Response = true);

            services.Configure<GlobalExceptionHandlerOptions>(options =>
            {
                options.MapDefaults();
                configure?.Invoke(options);
            });

            services.AddExceptionHandler<GlobalExceptionHandler>();

            // Without a catch block per action, this is what still sends the action's arguments with the error.
            services.Configure<MvcOptions>(options => options.Filters.Add<ActionArgumentsBreadcrumbFilter>());

            return services;
        }

        /// <summary>
        /// Uses the global exception handling. It must be the first middleware in the pipeline.
        /// </summary>
        /// <param name="app">The application builder.</param>
        /// <returns>The application builder.</returns>
        public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
        {
            ArgumentNullException.ThrowIfNull(app);

            return app.UseExceptionHandler();
        }
        #endregion
    }
}
