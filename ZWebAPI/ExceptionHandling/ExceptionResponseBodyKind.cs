namespace ZWebAPI.ExceptionHandling
{
    /// <summary>
    /// Describes how the body of an <see cref="ExceptionResponse"/> must be written to the HTTP response.
    /// </summary>
    public enum ExceptionResponseBodyKind
    {
        /// <summary>
        /// No body is written, only the status code.
        /// </summary>
        Empty = 0,

        /// <summary>
        /// An RFC 7807 problem details document is written as <c>application/problem+json</c>.
        /// </summary>
        ProblemDetails = 1,

        /// <summary>
        /// An arbitrary object is serialized as <c>application/json</c>.
        /// </summary>
        Content = 2,

        /// <summary>
        /// A raw string is written as <c>text/plain</c>.
        /// </summary>
        Text = 3,
    }
}
