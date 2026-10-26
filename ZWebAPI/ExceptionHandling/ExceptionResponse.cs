using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Describes the HTTP response produced for a mapped exception.
    /// </summary>
    /// <remarks>
    /// Instances are created through the static factory methods and are immutable apart from the
    /// fluent <see cref="Capture"/>, <see cref="DoNotCapture"/> and <see cref="WithLogLevel"/> modifiers.
    /// </remarks>
    public sealed class ExceptionResponse
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExceptionResponse"/> class.
        /// </summary>
        /// <param name="body">The response body, according to <paramref name="bodyKind"/>.</param>
        /// <param name="bodyKind">The kind of body to write.</param>
        /// <param name="statusCode">The HTTP status code.</param>
        private ExceptionResponse(object? body, ExceptionResponseBodyKind bodyKind, int statusCode)
        {
            Body = body;
            BodyKind = bodyKind;
            LogLevel = statusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Error : LogLevel.Warning;
            ShouldCapture = statusCode >= StatusCodes.Status500InternalServerError;
            StatusCode = statusCode;
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the response body, interpreted according to <see cref="BodyKind"/>.
        /// </summary>
        /// <value>
        /// The response body.
        /// </value>
        public object? Body { get; }

        /// <summary>
        /// Gets the kind of body to write to the HTTP response.
        /// </summary>
        /// <value>
        /// The body kind.
        /// </value>
        public ExceptionResponseBodyKind BodyKind { get; }

        /// <summary>
        /// Gets the level used when logging the exception.
        /// </summary>
        /// <value>
        /// The log level. Defaults to <see cref="LogLevel.Error"/> for status codes of 500 or above and
        /// <see cref="LogLevel.Warning"/> otherwise.
        /// </value>
        public LogLevel LogLevel { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the exception must be captured by the
        /// <see cref="IExceptionReporter"/> registered by the application.
        /// </summary>
        /// <value>
        /// <c>true</c> when the exception must be captured; otherwise, <c>false</c>. Defaults to <c>true</c>
        /// for status codes of 500 or above, because those represent faults rather than expected outcomes.
        /// </value>
        public bool ShouldCapture { get; private set; }

        /// <summary>
        /// Gets the HTTP status code to return.
        /// </summary>
        /// <value>
        /// The HTTP status code.
        /// </value>
        public int StatusCode { get; }
        #endregion

        #region Public methods

        /// <summary>
        /// Creates a response that serializes an arbitrary object as <c>application/json</c>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="body">The object to serialize.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse Content(int statusCode, object? body)
        {
            return new ExceptionResponse(body, ExceptionResponseBodyKind.Content, statusCode);
        }

        /// <summary>
        /// Creates a response with a status code and no body.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse Empty(int statusCode)
        {
            return new ExceptionResponse(null, ExceptionResponseBodyKind.Empty, statusCode);
        }

        /// <summary>
        /// Creates an RFC 7807 problem details response.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse Problem(int statusCode)
        {
            return Problem(statusCode, null, null);
        }

        /// <summary>
        /// Creates an RFC 7807 problem details response.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="title">The problem title, safe to expose to the client.</param>
        /// <param name="detail">The problem detail, safe to expose to the client.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse Problem(int statusCode, string? title, string? detail)
        {
            ProblemDetails problemDetails = new()
            {
                Detail = detail,
                Status = statusCode,
                Title = title,
            };

            return Problem(statusCode, problemDetails);
        }

        /// <summary>
        /// Creates an RFC 7807 problem details response from an existing document.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="problemDetails">The problem details document.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse Problem(int statusCode, ProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);

            problemDetails.Status ??= statusCode;

            return new ExceptionResponse(problemDetails, ExceptionResponseBodyKind.ProblemDetails, statusCode);
        }

        /// <summary>
        /// Creates a response that writes a raw string as <c>text/plain</c>.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="text">The text to write.</param>
        /// <returns>The exception response.</returns>
        /// <remarks>
        /// This reproduces what an MVC <c>StatusCode(code, "some-string")</c> produces, which the Angular
        /// clients rely on to read the body as a translation key.
        /// </remarks>
        public static ExceptionResponse Text(int statusCode, string text)
        {
            ArgumentNullException.ThrowIfNull(text);

            return new ExceptionResponse(text, ExceptionResponseBodyKind.Text, statusCode);
        }

        /// <summary>
        /// Creates a <c>400 Bad Request</c> validation problem details response.
        /// </summary>
        /// <param name="errors">The validation errors, keyed by field name.</param>
        /// <returns>The exception response.</returns>
        public static ExceptionResponse ValidationProblem(IDictionary<string, string[]> errors)
        {
            ArgumentNullException.ThrowIfNull(errors);

            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        /// <summary>
        /// Creates a <c>400 Bad Request</c> validation problem details response.
        /// </summary>
        /// <param name="problemDetails">The validation problem details document.</param>
        /// <returns>The exception response.</returns>
        /// <remarks>
        /// The serialized document keeps the <c>errors</c> dictionary at the root of the body, which is the
        /// shape the Angular <c>BackendFormValidationHelper</c> reads to map errors onto form controls.
        /// </remarks>
        public static ExceptionResponse ValidationProblem(ValidationProblemDetails problemDetails)
        {
            ArgumentNullException.ThrowIfNull(problemDetails);

            return Problem(StatusCodes.Status400BadRequest, problemDetails);
        }

        /// <summary>
        /// Forces the exception to be captured by the <see cref="IExceptionReporter"/>.
        /// </summary>
        /// <returns>The same instance, for chaining.</returns>
        public ExceptionResponse Capture()
        {
            ShouldCapture = true;
            return this;
        }

        /// <summary>
        /// Prevents the exception from being captured by the <see cref="IExceptionReporter"/>.
        /// </summary>
        /// <returns>The same instance, for chaining.</returns>
        public ExceptionResponse DoNotCapture()
        {
            ShouldCapture = false;
            return this;
        }

        /// <summary>
        /// Overrides the level used when logging the exception.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <returns>The same instance, for chaining.</returns>
        public ExceptionResponse WithLogLevel(LogLevel logLevel)
        {
            LogLevel = logLevel;
            return this;
        }
        #endregion
    }
}
