namespace ZWebAPI.Interfaces
{
    /// <summary>
    /// Reports exceptions to the application's error tracker, together with the breadcrumbs that lead to them.
    /// </summary>
    /// <remarks>
    /// The global exception handler and the action arguments breadcrumb filter resolve this service from the
    /// request scope, so breadcrumbs added during a request are sent with the exception captured at its end.
    /// Register it as scoped. When no implementation is registered, nothing is reported.
    /// </remarks>
    public interface IExceptionReporter
    {
        #region Public methods

        /// <summary>
        /// Adds a breadcrumb with a description and a set of parameters.
        /// </summary>
        /// <param name="description">The breadcrumb description.</param>
        /// <param name="parameters">The parameters recorded with the breadcrumb.</param>
        void AddBreadcrumb(string description, IDictionary<string, object?> parameters);

        /// <summary>
        /// Captures the exception, sending it with the breadcrumbs collected so far.
        /// </summary>
        /// <param name="exception">The exception to capture.</param>
        /// <returns>The identifier of the captured error, or <c>null</c> when it was not captured.</returns>
        Guid? CaptureException(Exception exception);
        #endregion
    }
}
