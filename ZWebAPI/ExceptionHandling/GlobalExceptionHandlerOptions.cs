using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZDatabase.Exceptions;
using ZSecurity.Exceptions;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Options for the <see cref="GlobalExceptionHandler"/>, including the exception to status code mappings.
    /// </summary>
    public sealed class GlobalExceptionHandlerOptions
    {
        #region Variables
        private readonly Dictionary<Type, Func<Exception, ExceptionMappingContext, ExceptionResponse>> mappings = [];
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether exceptions are reported through the
        /// <see cref="IExceptionReporter"/> registered by the application.
        /// </summary>
        /// <value>
        /// <c>true</c> to report exceptions; otherwise, <c>false</c>. Defaults to <c>true</c>.
        /// </value>
        /// <remarks>
        /// Individual mappings can still opt out through <see cref="ExceptionResponse.DoNotCapture"/>.
        /// </remarks>
        public bool IsCaptureEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the full exception detail (type, message, inner message and
        /// stack trace) is added to the problem details response when the host runs in the development environment.
        /// </summary>
        /// <value>
        /// <c>true</c> to include the exception detail in development; otherwise, <c>false</c>.
        /// Defaults to <c>false</c>, so nothing is leaked unless a consuming application explicitly opts in.
        /// </value>
        public bool IsExceptionDetailIncludedInDevelopment { get; set; }

        /// <summary>
        /// Gets or sets the detail returned for an exception that has no registered mapping.
        /// </summary>
        /// <value>
        /// The detail text. It must never contain internal information.
        /// </value>
        public string UnhandledExceptionDetail { get; set; } = "An unexpected error occurred while processing the request.";

        /// <summary>
        /// Gets or sets the title returned for an exception that has no registered mapping.
        /// </summary>
        /// <value>
        /// The title text. It must never contain internal information.
        /// </value>
        public string UnhandledExceptionTitle { get; set; } = "Internal Server Error";
        #endregion

        #region Public methods

        /// <summary>
        /// Removes every registered mapping, including the ones added by <see cref="MapDefaults"/>.
        /// </summary>
        /// <returns>The same instance, for chaining.</returns>
        public GlobalExceptionHandlerOptions ClearMappings()
        {
            mappings.Clear();
            return this;
        }

        /// <summary>
        /// Maps an exception type to a status code, returning an RFC 7807 problem details body.
        /// </summary>
        /// <typeparam name="TException">The exception type.</typeparam>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The same instance, for chaining.</returns>
        public GlobalExceptionHandlerOptions Map<TException>(int statusCode)
            where TException : Exception
        {
            return Map(typeof(TException), statusCode);
        }

        /// <summary>
        /// Maps an exception type through a custom factory.
        /// </summary>
        /// <typeparam name="TException">The exception type.</typeparam>
        /// <param name="mapper">The factory producing the response for the exception.</param>
        /// <returns>The same instance, for chaining.</returns>
        public GlobalExceptionHandlerOptions Map<TException>(Func<TException, ExceptionMappingContext, ExceptionResponse> mapper)
            where TException : Exception
        {
            ArgumentNullException.ThrowIfNull(mapper);

            return Map(typeof(TException), (exception, context) => mapper((TException)exception, context));
        }

        /// <summary>
        /// Maps an exception type to a status code, returning an RFC 7807 problem details body.
        /// </summary>
        /// <param name="exceptionType">
        /// The exception type. It can be a closed type (<c>EntityNotFoundException&lt;Employees&gt;</c>) or an
        /// open generic type definition (<c>typeof(EntityNotFoundException&lt;&gt;)</c>).
        /// </param>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>The same instance, for chaining.</returns>
        public GlobalExceptionHandlerOptions Map(Type exceptionType, int statusCode)
        {
            return Map(exceptionType, (_, _) => ExceptionResponse.Problem(statusCode));
        }

        /// <summary>
        /// Maps an exception type through a custom factory.
        /// </summary>
        /// <param name="exceptionType">
        /// The exception type. It can be a closed type (<c>EntityNotFoundException&lt;Employees&gt;</c>) or an
        /// open generic type definition (<c>typeof(EntityNotFoundException&lt;&gt;)</c>).
        /// </param>
        /// <param name="mapper">The factory producing the response for the exception.</param>
        /// <returns>The same instance, for chaining.</returns>
        /// <exception cref="ArgumentException">When <paramref name="exceptionType"/> is not an exception type.</exception>
        public GlobalExceptionHandlerOptions Map(Type exceptionType, Func<Exception, ExceptionMappingContext, ExceptionResponse> mapper)
        {
            ArgumentNullException.ThrowIfNull(exceptionType);
            ArgumentNullException.ThrowIfNull(mapper);

            if (exceptionType != typeof(Exception) && !exceptionType.IsSubclassOf(typeof(Exception)))
            {
                throw new ArgumentException($"The type '{exceptionType}' does not derive from '{typeof(Exception)}'.", nameof(exceptionType));
            }

            mappings[exceptionType] = mapper;
            return this;
        }

        /// <summary>
        /// Registers the mappings for the exceptions shipped by ZDatabase and ZSecurity.
        /// </summary>
        /// <returns>The same instance, for chaining.</returns>
        /// <remarks>
        /// <para>
        /// <see cref="MissingUserPermissionException"/> maps to <c>403 Forbidden</c>,
        /// <see cref="EntityNotFoundException{TEntity}"/> maps to <c>404 Not Found</c> and
        /// <see cref="EntityValidationFailureException{TKey}"/> maps to <c>400 Bad Request</c> with the
        /// <c>errors</c> dictionary and the <c>entityKey</c> at the root of the body.
        /// </para>
        /// <para>
        /// <see cref="ValidationFailureException"/> and <see cref="InvalidOperationException"/> are deliberately
        /// left out: the consuming applications give them application specific bodies (translation keys).
        /// </para>
        /// </remarks>
        public GlobalExceptionHandlerOptions MapDefaults()
        {
            return this
                .Map(typeof(EntityNotFoundException<>), StatusCodes.Status404NotFound)
                .Map(typeof(EntityValidationFailureException<>), MapEntityValidationFailure)
                .Map(typeof(MissingUserPermissionException), StatusCodes.Status403Forbidden);
        }
        #endregion

        #region Private methods

        /// <summary>
        /// Resolves the response for an exception, walking the exception type hierarchy from the most derived
        /// type up to <see cref="Exception"/> and honouring open generic registrations.
        /// </summary>
        /// <param name="context">The mapping context.</param>
        /// <returns>The mapped response, or <c>null</c> when no mapping matches.</returns>
        internal ExceptionResponse? Resolve(ExceptionMappingContext context)
        {
            Type? exceptionType = context.Exception.GetType();

            while (exceptionType is not null)
            {
                if (mappings.TryGetValue(exceptionType, out Func<Exception, ExceptionMappingContext, ExceptionResponse>? exactMapper))
                {
                    return exactMapper(context.Exception, context);
                }

                if (exceptionType.IsGenericType
                    && mappings.TryGetValue(exceptionType.GetGenericTypeDefinition(), out Func<Exception, ExceptionMappingContext, ExceptionResponse>? genericMapper))
                {
                    return genericMapper(context.Exception, context);
                }

                exceptionType = exceptionType.BaseType;
            }

            return null;
        }

        /// <summary>
        /// Builds the <c>400 Bad Request</c> response for an <see cref="EntityValidationFailureException{TKey}"/>,
        /// reading the entity key through reflection because the key type is not known at this level.
        /// </summary>
        /// <param name="exception">The exception.</param>
        /// <param name="context">The mapping context.</param>
        /// <returns>The exception response.</returns>
        private static ExceptionResponse MapEntityValidationFailure(Exception exception, ExceptionMappingContext context)
        {
            ValidationFailureException validationException = (ValidationFailureException)exception;

            ValidationProblemDetails problemDetails = new(validationException.ValidationResult.Errors);

            PropertyInfo? entityKeyProperty = exception.GetType().GetProperty(nameof(EntityValidationFailureException<int>.EntityKey));
            if (entityKeyProperty?.GetValue(exception) is object entityKey)
            {
                problemDetails.Extensions[GlobalExceptionHandler.EntityKeyExtensionKey] = entityKey;
            }

            return ExceptionResponse.ValidationProblem(problemDetails);
        }
        #endregion
    }
}
