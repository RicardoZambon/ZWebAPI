using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using AspNetCoreExceptionHandler = Microsoft.AspNetCore.Diagnostics.IExceptionHandler;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Global exception handler that converts any unhandled exception into a safe RFC 7807 response while the
    /// full detail is logged and captured through the <see cref="IExceptionReporter"/> the application registers.
    /// </summary>
    /// <seealso cref="AspNetCoreExceptionHandler" />
    public sealed class GlobalExceptionHandler : AspNetCoreExceptionHandler
    {
        #region Variables

        /// <summary>
        /// The problem details extension holding the key of the entity that failed validation.
        /// </summary>
        /// <remarks>
        /// Extension keys are serialized verbatim, so the name is written camelCased: it is the name the frontend
        /// reads (<c>error.entityKey</c>) and the one an <c>EntityKey</c> property gets under the web JSON defaults.
        /// </remarks>
        public const string EntityKeyExtensionKey = "entityKey";

        /// <summary>
        /// The problem details extension holding the error identifier returned by the reporter.
        /// </summary>
        public const string ErrorIdExtensionKey = "errorId";

        /// <summary>
        /// The problem details extension holding the exception detail, only present in development.
        /// </summary>
        public const string ExceptionExtensionKey = "exception";

        /// <summary>
        /// The problem details extension holding the request correlation identifier.
        /// </summary>
        public const string TraceIdExtensionKey = "traceId";

        private readonly IHostEnvironment hostEnvironment;
        private readonly ILogger<GlobalExceptionHandler> logger;
        private readonly GlobalExceptionHandlerOptions options;
        private readonly IProblemDetailsService problemDetailsService;
        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="GlobalExceptionHandler"/> class.
        /// </summary>
        /// <param name="hostEnvironment">The <see cref="IHostEnvironment"/> instance.</param>
        /// <param name="logger">The <see cref="ILogger{TCategoryName}"/> instance.</param>
        /// <param name="options">The <see cref="GlobalExceptionHandlerOptions"/> instance.</param>
        /// <param name="problemDetailsService">The <see cref="IProblemDetailsService"/> instance.</param>
        public GlobalExceptionHandler(
            IHostEnvironment hostEnvironment,
            ILogger<GlobalExceptionHandler> logger,
            IOptions<GlobalExceptionHandlerOptions> options,
            IProblemDetailsService problemDetailsService)
        {
            this.hostEnvironment = hostEnvironment;
            this.logger = logger;
            this.options = options.Value;
            this.problemDetailsService = problemDetailsService;
        }
        #endregion

        #region Public methods

        /// <inheritdoc />
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(httpContext);
            ArgumentNullException.ThrowIfNull(exception);

            if (httpContext.Response.HasStarted)
            {
                // The body is already on the wire, there is nothing safe left to write.
                logger.LogError(exception, "An exception was thrown after the response had started, so it could not be handled.");
                return false;
            }

            bool isDevelopment = hostEnvironment.IsDevelopment();
            string traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

            ExceptionMappingContext mappingContext = new(exception, httpContext, isDevelopment, traceId);

            ExceptionResponse response = options.Resolve(mappingContext)
                ?? ExceptionResponse.Problem(StatusCodes.Status500InternalServerError, options.UnhandledExceptionTitle, options.UnhandledExceptionDetail);

            Guid? errorId = CaptureException(httpContext, exception, response, traceId);

            logger.Log(
                response.LogLevel,
                exception,
                "Request {RequestMethod} {RequestPath} failed with status code {StatusCode}. TraceId: {TraceId}. ErrorId: {ErrorId}.",
                httpContext.Request.Method,
                httpContext.Request.Path.Value,
                response.StatusCode,
                traceId,
                errorId);

            httpContext.Response.StatusCode = response.StatusCode;

            await WriteBodyAsync(httpContext, exception, response, errorId, isDevelopment, traceId);

            return true;
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Builds the exception detail exposed in development only.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <returns>The exception detail.</returns>
        private static Dictionary<string, string?> BuildExceptionDetail(Exception exception)
        {
            return new Dictionary<string, string?>
            {
                { "innerMessage", exception.InnerException?.Message },
                { "message", exception.Message },
                { "stackTrace", exception.StackTrace },
                { "type", exception.GetType().FullName },
            };
        }

        /// <summary>
        /// Captures the exception through the <see cref="IExceptionReporter"/> when the response asks for it.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="response">The resolved response.</param>
        /// <param name="traceId">The request correlation identifier.</param>
        /// <returns>The error identifier returned by the reporter, or <c>null</c> when the exception was not captured.</returns>
        private Guid? CaptureException(HttpContext httpContext, Exception exception, ExceptionResponse response, string traceId)
        {
            if (!options.IsCaptureEnabled || !response.ShouldCapture)
            {
                return null;
            }

            if (httpContext.RequestServices.GetService<IExceptionReporter>() is not IExceptionReporter exceptionHandler)
            {
                return null;
            }

            try
            {
                exceptionHandler.AddBreadcrumb(
                    "Unhandled exception reached the global exception handler.",
                    new Dictionary<string, object?>
                    {
                        { "Method", httpContext.Request.Method },
                        { "Path", httpContext.Request.Path.Value },
                        { "QueryString", httpContext.Request.QueryString.Value },
                        { "TraceId", traceId },
                    });

                return exceptionHandler.CaptureException(exception);
            }
            catch (Exception captureException)
            {
                // Reporting must never replace the response the client is waiting for.
                logger.LogError(captureException, "Failed to capture the exception. TraceId: {TraceId}.", traceId);
                return null;
            }
        }

        /// <summary>
        /// Adds the correlation identifier, the error identifier and, in development, the exception detail.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="errorId">The error identifier returned by the reporter.</param>
        /// <param name="isDevelopment">Whether the host runs in the development environment.</param>
        /// <param name="problemDetails">The problem details document.</param>
        /// <param name="traceId">The request correlation identifier.</param>
        private void EnrichProblemDetails(Exception exception, Guid? errorId, bool isDevelopment, ProblemDetails problemDetails, string traceId)
        {
            problemDetails.Extensions[TraceIdExtensionKey] = traceId;

            if (errorId is Guid capturedErrorId)
            {
                problemDetails.Extensions[ErrorIdExtensionKey] = capturedErrorId;
            }

            if (isDevelopment && options.IsExceptionDetailIncludedInDevelopment)
            {
                problemDetails.Extensions[ExceptionExtensionKey] = BuildExceptionDetail(exception);
            }
        }

        /// <summary>
        /// Writes the response body according to the resolved <see cref="ExceptionResponse"/>.
        /// </summary>
        /// <param name="httpContext">The HTTP context.</param>
        /// <param name="exception">The exception.</param>
        /// <param name="response">The resolved response.</param>
        /// <param name="errorId">The error identifier returned by the reporter.</param>
        /// <param name="isDevelopment">Whether the host runs in the development environment.</param>
        /// <param name="traceId">The request correlation identifier.</param>
        /// <returns>A task that completes when the body has been written.</returns>
        private async Task WriteBodyAsync(HttpContext httpContext, Exception exception, ExceptionResponse response, Guid? errorId, bool isDevelopment, string traceId)
        {
            switch (response.BodyKind)
            {
                case ExceptionResponseBodyKind.ProblemDetails:
                    ProblemDetails problemDetails = (ProblemDetails)response.Body!;

                    problemDetails.Instance ??= httpContext.Request.Path;
                    problemDetails.Status = response.StatusCode;

                    EnrichProblemDetails(exception, errorId, isDevelopment, problemDetails, traceId);

                    bool isWritten = await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
                    {
                        Exception = exception,
                        HttpContext = httpContext,
                        ProblemDetails = problemDetails,
                    });

                    if (!isWritten)
                    {
                        await httpContext.Response.WriteAsJsonAsync(problemDetails, problemDetails.GetType());
                    }

                    break;

                case ExceptionResponseBodyKind.Content:
                    await httpContext.Response.WriteAsJsonAsync(response.Body);
                    break;

                case ExceptionResponseBodyKind.Text:
                    httpContext.Response.ContentType = "text/plain; charset=utf-8";
                    await httpContext.Response.WriteAsync((string)response.Body!);
                    break;

                default:
                    break;
            }
        }
        #endregion
    }
}
