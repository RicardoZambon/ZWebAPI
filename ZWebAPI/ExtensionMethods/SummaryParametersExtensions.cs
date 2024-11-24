using System.Text.Json;
using ZWebAPI.Interfaces;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="ZWebAPI.Interfaces.ISummaryParameters"/>.
    /// </summary>
    public static class SummaryParametersExtensions
    {
        /// <summary>
        /// Determines whether the summary parameters has filters.
        /// </summary>
        /// <param name="parameters">The summary parameters.</param>
        /// <returns>
        ///   <c>true</c> if the specified summary parameters has filters; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFilters(this ISummaryParameters parameters)
        {
            return parameters?.Filters?.Any() ?? false;
        }

        /// <summary>
        /// Determines whether the summary parameter has a specific property filter.
        /// </summary>
        /// <param name="parameters">The summary parameters.</param>
        /// <param name="property">The property.</param>
        /// <returns>
        ///   <c>true</c> if the specified summary parameter has the property filter; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFilter(this ISummaryParameters parameters, string property)
        {
            return parameters?.Filters?.Keys?.Any(x => string.Equals(x, property, StringComparison.OrdinalIgnoreCase)) ?? false;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="parameters">The parameters.</param>
        /// <param name="property">The property.</param>
        /// <returns>The filter value.</returns>
        public static TResult? GetFilterValue<TResult>(this ISummaryParameters parameters, string property)
            where TResult : struct
        {
            if (parameters.GetFilterValue(property, typeof(TResult)) is object result)
            {
                return (TResult)result;
            }
            return null;
        }

        /// <summary>
        /// Gets the filter value.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="property">The property.</param>
        /// <param name="resultType">Type of the result.</param>
        /// <returns>The filter value.</returns>
        public static object? GetFilterValue(this ISummaryParameters parameters, string property, Type resultType)
        {
            KeyValuePair<string, object>? filter = parameters.Filters?.FirstOrDefault(x => string.Equals(x.Key, property, StringComparison.OrdinalIgnoreCase));

            if (filter?.Value is JsonElement jsonValue)
            {
                if (jsonValue.ValueKind == JsonValueKind.Null)
                {
                    return null;
                }

                switch (Type.GetTypeCode(resultType))
                {
                    case TypeCode.Boolean:
                        if (jsonValue.ValueKind == JsonValueKind.String && bool.TryParse(jsonValue.GetString(), out bool boolResult))
                        {
                            return boolResult;
                        }
                        return jsonValue.GetBoolean();

                    case TypeCode.Byte:
                        if (jsonValue.ValueKind == JsonValueKind.String && byte.TryParse(jsonValue.GetString(), out byte byteResult))
                        {
                            return byteResult;
                        }
                        return jsonValue.GetByte();

                    case TypeCode.DateTime:
                        if (jsonValue.ValueKind == JsonValueKind.String && DateTime.TryParse(jsonValue.GetString(), out DateTime dateTimeResult))
                        {
                            return dateTimeResult;
                        }
                        return jsonValue.GetDateTime();

                    case TypeCode.Decimal:
                        if (jsonValue.ValueKind == JsonValueKind.String && decimal.TryParse(jsonValue.GetString(), out decimal decimalTimeResult))
                        {
                            return decimalTimeResult;
                        }
                        return jsonValue.GetDecimal();

                    case TypeCode.Double:
                        if (jsonValue.ValueKind == JsonValueKind.String && double.TryParse(jsonValue.GetString(), out double doubleResult))
                        {
                            return doubleResult;
                        }
                        return jsonValue.GetDouble();

                    case TypeCode.Int16:
                        if (jsonValue.ValueKind == JsonValueKind.String && short.TryParse(jsonValue.GetString(), out short shortResult))
                        {
                            return shortResult;
                        }
                        return jsonValue.GetInt16();

                    case TypeCode.Int32:
                        if (jsonValue.ValueKind == JsonValueKind.String && int.TryParse(jsonValue.GetString(), out int intResult))
                        {
                            return intResult;
                        }
                        return jsonValue.GetInt32();

                    case TypeCode.Int64:
                        if (jsonValue.ValueKind == JsonValueKind.String && long.TryParse(jsonValue.GetString(), out long longResult))
                        {
                            return longResult;
                        }
                        return jsonValue.GetInt64();

                    case TypeCode.SByte:
                        if (jsonValue.ValueKind == JsonValueKind.String && sbyte.TryParse(jsonValue.GetString(), out sbyte sbyteResult))
                        {
                            return sbyteResult;
                        }
                        return jsonValue.GetSByte();

                    case TypeCode.Single:
                        if (jsonValue.ValueKind == JsonValueKind.String && float.TryParse(jsonValue.GetString(), out float floatResult))
                        {
                            return floatResult;
                        }
                        return jsonValue.GetSingle();

                    case TypeCode.UInt16:
                        if (jsonValue.ValueKind == JsonValueKind.String && ushort.TryParse(jsonValue.GetString(), out ushort ushortResult))
                        {
                            return ushortResult;
                        }
                        return jsonValue.GetInt16();

                    case TypeCode.UInt32:
                        if (jsonValue.ValueKind == JsonValueKind.String && uint.TryParse(jsonValue.GetString(), out uint uintResult))
                        {
                            return uintResult;
                        }
                        return jsonValue.GetInt32();

                    case TypeCode.UInt64:
                        if (jsonValue.ValueKind == JsonValueKind.String && ulong.TryParse(jsonValue.GetString(), out ulong ulongResult))
                        {
                            return ulongResult;
                        }
                        return jsonValue.GetInt64();

                    case TypeCode.String:
                        return jsonValue.GetString();

                    default:
                        object rawValue = jsonValue.GetRawText();
                        if (rawValue is null)
                        {
                            return null;
                        }

                        if (resultType.IsEnum)
                        {
                            return Enum.Parse(resultType, rawValue.ToString() ?? string.Empty);
                        }
                        return Convert.ChangeType(rawValue, resultType);
                }
            }
            return null;
        }

        /// <summary>
        /// Updates the filter value.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        public static void UpdateFilterValue(this ISummaryParameters parameters, string property, object value)
        {
            if (parameters?.Filters?.Keys?.FirstOrDefault(x => string.Equals(x, property, StringComparison.OrdinalIgnoreCase)) is string key)
            {
                parameters.Filters[key] = JsonDocument.Parse(JsonSerializer.Serialize(value)).RootElement;
            }
        }
    }
}