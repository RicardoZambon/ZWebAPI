using Microsoft.AspNetCore.Http;

namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Contextual information handed to an exception mapper.
    /// </summary>
    public sealed class ExceptionMappingContext
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionMappingContext"/> class.
        /// </summary>
        /// <param name="exception">The exception being handled.</param>
        /// <param name="httpContext">The <see cref="HttpContext"/> of the failed request.</param>
        /// <param name="isDevelopment">Whether the host is running in the development environment.</param>
        /// <param name="traceId">The correlation identifier of the failed request.</param>
        public ExceptionMappingContext(
            Exception exception,
            HttpContext httpContext,
            bool isDevelopment,
            string traceId)
        {
            Exception = exception;
            HttpContext = httpContext;
            IsDevelopment = isDevelopment;
            TraceId = traceId;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the exception being handled.
        /// </summary>
        /// <value>
        /// The exception.
        /// </value>
        public Exception Exception { get; }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> of the failed request.
        /// </summary>
        /// <value>
        /// The HTTP context.
        /// </value>
        public HttpContext HttpContext { get; }

        /// <summary>
        /// Gets a value indicating whether the host is running in the development environment.
        /// </summary>
        /// <value>
        /// <c>true</c> when running in development; otherwise, <c>false</c>.
        /// </value>
        public bool IsDevelopment { get; }

        /// <summary>
        /// Gets the correlation identifier of the failed request.
        /// </summary>
        /// <value>
        /// The <c>Activity.Current?.Id</c> when available, otherwise the <c>HttpContext.TraceIdentifier</c>.
        /// </value>
        public string TraceId { get; }
        #endregion
    }
}
