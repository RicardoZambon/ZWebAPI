using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZDatabase.Exceptions;
using ZDatabase.Validations;
using ZSecurity.Exceptions;
using ZWebAPI.ExceptionHandling;

namespace ZWebAPI.UnitTests.ExceptionHandling
{
    /// <summary>
    /// Unit tests for <see cref="GlobalExceptionHandlerOptions"/>.
    /// </summary>
    public class GlobalExceptionHandlerOptionsTests
    {
        /// <summary>
        /// Test the Map should refuse a type that is not an exception.
        /// </summary>
        [Fact]
        public void Map_Fail_RefusesATypeThatIsNotAnException()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new();

            // Act
            Action act = () => options.Map(typeof(string), StatusCodes.Status400BadRequest);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        /// <summary>
        /// Test the MapDefaults should answer 400 with the errors and the entity key of a failed entity validation.
        /// </summary>
        [Fact]
        public void MapDefaults_Pass_AnswersEntityValidationFailureWithErrorsAndEntityKey()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions().MapDefaults();
            ValidationResult validationResult = new();
            validationResult.SetError("Name", "Required");
            EntityValidationFailureException<long> exception = new("Entity", 42L, validationResult);

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(exception));

            // Assert
            response.Should().NotBeNull();
            response!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            ValidationProblemDetails problemDetails = response.Body.Should().BeOfType<ValidationProblemDetails>().Subject;
            problemDetails.Errors.Should().ContainKey("Name");
            problemDetails.Extensions[GlobalExceptionHandler.EntityKeyExtensionKey].Should().Be(42L);
        }

        /// <summary>
        /// Test the MapDefaults should answer 404 for a missing entity, whatever its entity type.
        /// </summary>
        [Fact]
        public void MapDefaults_Pass_AnswersEntityNotFoundWith404()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions().MapDefaults();

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(new EntityNotFoundException<string>(1)));

            // Assert
            response!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            response.ShouldCapture.Should().BeFalse();
        }

        /// <summary>
        /// Test the MapDefaults should answer 403 for a missing permission.
        /// </summary>
        [Fact]
        public void MapDefaults_Pass_AnswersMissingUserPermissionWith403()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions().MapDefaults();

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(new MissingUserPermissionException("Action")));

            // Assert
            response!.StatusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        /// <summary>
        /// Test the Resolve should fall back to the mapping of a base type.
        /// </summary>
        [Fact]
        public void Resolve_Pass_UsesTheMappingOfABaseType()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions()
                .Map<ArgumentException>(StatusCodes.Status400BadRequest);

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(new ArgumentNullException("value")));

            // Assert
            response!.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }

        /// <summary>
        /// Test the Resolve should prefer the mapping of the exact type over the one of its base type.
        /// </summary>
        [Fact]
        public void Resolve_Pass_PrefersTheExactTypeOverItsBaseType()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions()
                .Map<ArgumentException>(StatusCodes.Status400BadRequest)
                .Map<ArgumentNullException>((exception, _) => ExceptionResponse.Content(StatusCodes.Status422UnprocessableEntity, exception.ParamName));

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(new ArgumentNullException("value")));

            // Assert
            response!.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
            response.BodyKind.Should().Be(ExceptionResponseBodyKind.Content);
            response.Body.Should().Be("value");
        }

        /// <summary>
        /// Test the Resolve should answer nothing for an exception without a mapping.
        /// </summary>
        [Fact]
        public void Resolve_Pass_ReturnsNullWhenNothingIsMapped()
        {
            // Arrange
            GlobalExceptionHandlerOptions options = new GlobalExceptionHandlerOptions().ClearMappings();

            // Act
            ExceptionResponse? response = options.Resolve(CreateContext(new InvalidOperationException()));

            // Assert
            response.Should().BeNull();
        }

        private static ExceptionMappingContext CreateContext(Exception exception)
            => new(exception, new DefaultHttpContext(), false, "trace");
    }
}
