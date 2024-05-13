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
                switch (jsonValue.ValueKind)
                {
                    case JsonValueKind.False:
                    case JsonValueKind.True:
                        return jsonValue.GetBoolean();
                    case JsonValueKind.Null:
                        return null;
                    case JsonValueKind.Number:
                        if (resultType == typeof(byte))
                        {
                            return jsonValue.GetByte();
                        }
                        else if (resultType == typeof(decimal))
                        {
                            return jsonValue.GetDecimal();
                        }
                        else if (resultType == typeof(double))
                        {
                            return jsonValue.GetDouble();
                        }
                        else if (resultType == typeof(float))
                        {
                            return jsonValue.GetSingle();
                        }
                        else if (resultType == typeof(long))
                        {
                            return jsonValue.GetInt64();
                        }
                        else if (resultType == typeof(sbyte))
                        {
                            return jsonValue.GetSByte();
                        }
                        else if (resultType == typeof(short))
                        {
                            return jsonValue.GetInt16();
                        }
                        else if (resultType == typeof(uint))
                        {
                            return jsonValue.GetUInt32();
                        }
                        else if (resultType == typeof(ulong))
                        {
                            return jsonValue.GetUInt64();
                        }
                        else if (resultType == typeof(ushort))
                        {
                            return jsonValue.GetUInt16();
                        }
                        return jsonValue.GetInt32();

                    case JsonValueKind.String:
                        return jsonValue.Deserialize(resultType);

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