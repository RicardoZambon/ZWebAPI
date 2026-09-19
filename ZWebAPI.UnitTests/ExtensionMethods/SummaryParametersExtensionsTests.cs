using System.Text.Json;
using ZWebAPI.ExtensionMethods;
using ZWebAPI.Interfaces;
using ZWebAPI.Models;
using ZWebAPI.UnitTests.Fakes.EntitiesFake;

namespace ZWebAPI.UnitTests.ExtensionMethods
{
    /// <summary>
    /// Unit tests for <see cref="ZWebAPI.ExtensionMethods.SummaryParametersExtensions"/>.
    /// </summary>
    public class SummaryParametersExtensionsTests
    {
        /// <summary>
        /// Test the HasFilters should report the presence of at least one filter.
        /// </summary>
        [Fact]
        public void HasFilters_Pass_TrueWhenAtLeastOneFilterIsPresent()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Name", "\"abc\""));

            // Act
            bool result = parameters.HasFilters();

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Test the HasFilters should report false when the filter dictionary is empty.
        /// </summary>
        [Fact]
        public void HasFilters_Pass_FalseWhenTheFilterDictionaryIsEmpty()
        {
            // Arrange
            ISummaryParameters parameters = Build();

            // Act
            bool result = parameters.HasFilters();

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasFilters should report false when the filter dictionary is null.
        /// </summary>
        [Fact]
        public void HasFilters_Pass_FalseWhenTheFilterDictionaryIsNull()
        {
            // Arrange
            ISummaryParameters parameters = new SummaryParametersModel { Filters = null };

            // Act
            bool result = parameters.HasFilters();

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasFilters should report false when the parameters themselves are null.
        /// </summary>
        [Fact]
        public void HasFilters_Pass_FalseWhenTheParametersAreNull()
        {
            // Arrange
            ISummaryParameters? parameters = null;

            // Act
            bool result = parameters!.HasFilters();

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasFilter should match the property name ignoring case.
        /// </summary>
        /// <param name="property">The property name being looked up.</param>
        [Theory]
        [InlineData("Name")]
        [InlineData("name")]
        [InlineData("NAME")]
        public void HasFilter_Pass_MatchesThePropertyIgnoringCase(string property)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Name", "\"abc\""));

            // Act
            bool result = parameters.HasFilter(property);

            // Assert
            result.Should().BeTrue();
        }

        /// <summary>
        /// Test the HasFilter should report false for a property that was not sent.
        /// </summary>
        [Fact]
        public void HasFilter_Pass_FalseForAPropertyThatWasNotSent()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Name", "\"abc\""));

            // Act
            bool result = parameters.HasFilter("Age");

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasFilter should report false when the filter dictionary is null.
        /// </summary>
        [Fact]
        public void HasFilter_Pass_FalseWhenTheFilterDictionaryIsNull()
        {
            // Arrange
            ISummaryParameters parameters = new SummaryParametersModel { Filters = null };

            // Act
            bool result = parameters.HasFilter("Name");

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the HasFilter should report false when the parameters themselves are null.
        /// </summary>
        [Fact]
        public void HasFilter_Pass_FalseWhenTheParametersAreNull()
        {
            // Arrange
            ISummaryParameters? parameters = null;

            // Act
            bool result = parameters!.HasFilter("Name");

            // Assert
            result.Should().BeFalse();
        }

        /// <summary>
        /// Test the GetFilterValue should read a JSON literal into the requested primitive type.
        /// </summary>
        /// <param name="json">The raw JSON value sent by the client.</param>
        /// <param name="resultType">The type the caller expects back.</param>
        /// <param name="expected">The expected value.</param>
        [Theory]
        [InlineData("true", typeof(bool), true)]
        [InlineData("false", typeof(bool), false)]
        [InlineData("5", typeof(byte), (byte)5)]
        [InlineData("1.5", typeof(double), 1.5d)]
        [InlineData("5", typeof(short), (short)5)]
        [InlineData("5", typeof(int), 5)]
        [InlineData("5", typeof(long), 5L)]
        [InlineData("-5", typeof(sbyte), (sbyte)-5)]
        [InlineData("1.5", typeof(float), 1.5f)]
        [InlineData("\"abc\"", typeof(string), "abc")]
        public void GetFilterValue_Pass_ReadsJsonLiteralIntoTheRequestedType(string json, Type resultType, object expected)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", json));

            // Act
            object? result = parameters.GetFilterValue("Value", resultType);

            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Test the GetFilterValue should read a decimal JSON literal.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReadsDecimalJsonLiteral()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "1.5"));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(decimal));

            // Assert
            result.Should().Be(1.5m);
        }

        /// <summary>
        /// Test the GetFilterValue should read a JSON date literal.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReadsDateTimeJsonLiteral()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "\"2024-03-09T21:45:00\""));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(DateTime));

            // Assert
            result.Should().Be(new DateTime(2024, 3, 9, 21, 45, 0));
        }

        /// <summary>
        /// Test the GetFilterValue should parse a quoted value into the requested primitive type.
        /// </summary>
        /// <param name="json">The raw JSON string sent by the client.</param>
        /// <param name="resultType">The type the caller expects back.</param>
        /// <param name="expected">The expected value.</param>
        [Theory]
        [InlineData("\"true\"", typeof(bool), true)]
        [InlineData("\"5\"", typeof(byte), (byte)5)]
        [InlineData("\"5\"", typeof(double), 5d)]
        [InlineData("\"5\"", typeof(short), (short)5)]
        [InlineData("\"5\"", typeof(int), 5)]
        [InlineData("\"5\"", typeof(long), 5L)]
        [InlineData("\"-5\"", typeof(sbyte), (sbyte)-5)]
        [InlineData("\"5\"", typeof(float), 5f)]
        public void GetFilterValue_Pass_ParsesQuotedValuesIntoTheRequestedType(string json, Type resultType, object expected)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", json));

            // Act
            object? result = parameters.GetFilterValue("Value", resultType);

            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Test the GetFilterValue should parse a quoted decimal value.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ParsesQuotedDecimalValue()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "\"5\""));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(decimal));

            // Assert
            result.Should().Be(5m);
        }

        /// <summary>
        /// Test the GetFilterValue should parse a quoted value into the requested unsigned type.
        /// </summary>
        /// <param name="resultType">The unsigned type the caller expects back.</param>
        [Theory]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public void GetFilterValue_Pass_ParsesQuotedValuesIntoUnsignedTypes(Type resultType)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "\"5\""));

            // Act
            object? result = parameters.GetFilterValue("Value", resultType);

            // Assert
            result.Should().Be(Convert.ChangeType(5, resultType));
        }

        /// <summary>
        /// Test the GetFilterValue should read an unsigned JSON literal.
        /// </summary>
        /// <param name="resultType">The unsigned type the caller expects back.</param>
        [Theory]
        [InlineData(typeof(ushort))]
        [InlineData(typeof(uint))]
        [InlineData(typeof(ulong))]
        public void GetFilterValue_Pass_ReadsUnsignedJsonLiteral(Type resultType)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "5"));

            // Act
            object? result = parameters.GetFilterValue("Value", resultType);

            // Assert
            Convert.ToUInt64(result).Should().Be(5UL);
        }

        /// <summary>
        /// Test the GetFilterValue should read a JSON literal into a nullable property type.
        /// </summary>
        /// <param name="json">The raw JSON value sent by the client.</param>
        /// <param name="resultType">The nullable type the caller expects back.</param>
        /// <param name="expected">The expected unwrapped value.</param>
        [Theory]
        [InlineData("5", typeof(long?), 5L)]
        [InlineData("\"5\"", typeof(long?), 5L)]
        [InlineData("5", typeof(int?), 5)]
        [InlineData("true", typeof(bool?), true)]
        [InlineData("\"abc\"", typeof(string), "abc")]
        public void GetFilterValue_Pass_UnwrapsNullableTargetTypes(string json, Type resultType, object expected)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", json));

            // Act
            object? result = parameters.GetFilterValue("Value", resultType);

            // Assert
            result.Should().Be(expected);
        }

        /// <summary>
        /// Test the GetFilterValue should read a nullable enum filter as its underlying value.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_UnwrapsNullableEnumTargetType()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "1"));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(StatusFake?));

            // Assert
            result.Should().Be((int)StatusFake.Active);
        }

        /// <summary>
        /// Test the GetFilterValue should read an enum filter as its underlying value, which the
        /// caller can unbox straight into the enum.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReadsEnumFilterAsItsUnderlyingValue()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "2"));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(StatusFake));

            // Assert
            result.Should().Be((int)StatusFake.Inactive);
            parameters.GetFilterValue<StatusFake>("Value").Should().Be(StatusFake.Inactive);
        }

        /// <summary>
        /// Test the GetFilterValue should convert the raw JSON text for the remaining target types.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ConvertsTheRawJsonTextForOtherTargetTypes()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "9"));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(char));

            // Assert
            result.Should().Be('9');
        }

        /// <summary>
        /// Test the GetFilterValue should return null for an explicit JSON null.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReturnsNullForAnExplicitJsonNull()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "null"));

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(long?));

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Test the GetFilterValue should return null for a property that was not sent.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReturnsNullForAPropertyThatWasNotSent()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "5"));

            // Act
            object? result = parameters.GetFilterValue("Other", typeof(long));

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Test the GetFilterValue should return null when the filter dictionary is null.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReturnsNullWhenTheFilterDictionaryIsNull()
        {
            // Arrange
            ISummaryParameters parameters = new SummaryParametersModel { Filters = null };

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(long));

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Test the GetFilterValue should return null when the stored value is not a JSON element.
        /// </summary>
        [Fact]
        public void GetFilterValue_Pass_ReturnsNullWhenTheStoredValueIsNotJson()
        {
            // Arrange
            ISummaryParameters parameters = new SummaryParametersModel
            {
                Filters = new Dictionary<string, object> { ["Value"] = 5L },
            };

            // Act
            object? result = parameters.GetFilterValue("Value", typeof(long));

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Test the GetFilterValue should throw when a date filter does not carry a date.
        /// </summary>
        [Fact]
        public void GetFilterValue_Fail_ThrowsWhenADateFilterDoesNotCarryADate()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "\"not a date\""));

            // Act
            Action act = () => parameters.GetFilterValue("Value", typeof(DateTime));

            // Assert
            act.Should().Throw<FormatException>();
        }

        /// <summary>
        /// Test the generic GetFilterValue should return the typed value.
        /// </summary>
        [Fact]
        public void GetFilterValueGeneric_Pass_ReturnsTheTypedValue()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "5"));

            // Act
            long? result = parameters.GetFilterValue<long>("Value");

            // Assert
            result.Should().Be(5L);
        }

        /// <summary>
        /// Test the generic GetFilterValue should return null when the filter is absent or explicitly null.
        /// </summary>
        /// <param name="property">The property being looked up.</param>
        [Theory]
        [InlineData("Value")]
        [InlineData("Other")]
        public void GetFilterValueGeneric_Pass_ReturnsNullWhenThereIsNoValue(string property)
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "null"));

            // Act
            long? result = parameters.GetFilterValue<long>(property);

            // Assert
            result.Should().BeNull();
        }

        /// <summary>
        /// Test the UpdateFilterValue should replace the stored value, matching the key ignoring case.
        /// </summary>
        [Fact]
        public void UpdateFilterValue_Pass_ReplacesTheStoredValue()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "5"));

            // Act
            parameters.UpdateFilterValue("value", 9L);

            // Assert
            parameters.GetFilterValue("Value", typeof(long)).Should().Be(9L);
        }

        /// <summary>
        /// Test the UpdateFilterValue should leave the filters untouched when the property is absent.
        /// </summary>
        [Fact]
        public void UpdateFilterValue_Pass_LeavesTheFiltersUntouchedForAnAbsentProperty()
        {
            // Arrange
            ISummaryParameters parameters = Build(("Value", "5"));

            // Act
            parameters.UpdateFilterValue("Other", 9L);

            // Assert
            parameters.Filters.Should().ContainSingle();
            parameters.GetFilterValue("Value", typeof(long)).Should().Be(5L);
        }

        /// <summary>
        /// Test the UpdateFilterValue should do nothing when the filter dictionary is null.
        /// </summary>
        [Fact]
        public void UpdateFilterValue_Pass_DoesNothingWhenTheFilterDictionaryIsNull()
        {
            // Arrange
            ISummaryParameters parameters = new SummaryParametersModel { Filters = null };

            // Act
            Action act = () => parameters.UpdateFilterValue("Value", 9L);

            // Assert
            act.Should().NotThrow();
            parameters.Filters.Should().BeNull();
        }

        private static SummaryParametersModel Build(params (string Key, string Json)[] filters)
        {
            SummaryParametersModel parameters = new();

            foreach ((string key, string json) in filters)
            {
                parameters.Filters![key] = JsonDocument.Parse(json).RootElement;
            }

            return parameters;
        }
    }
}
