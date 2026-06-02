using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Resolves <see cref="ExportColumn"/> metadata for a row type by reflecting its public
    /// readable instance properties. Honors <see cref="ExportColumnAttribute"/> overrides
    /// (custom header, ignore, order, type) and caches the result per type.
    /// </summary>
    public static class ColumnMetadataResolver
    {
        #region Variables
        private static readonly ConcurrentDictionary<Type, IReadOnlyList<ExportColumn>> Cache = new();
        #endregion

        #region Public methods
        /// <summary>
        /// Resolves columns for <paramref name="rowType"/>.
        /// </summary>
        public static IReadOnlyList<ExportColumn> Resolve(Type rowType)
        {
            ArgumentNullException.ThrowIfNull(rowType);

            return Cache.GetOrAdd(rowType, BuildColumns);
        }
        #endregion

        #region Private methods
        private static IReadOnlyList<ExportColumn> BuildColumns(Type rowType)
        {
            PropertyInfo[] properties = rowType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            List<(int DeclarationOrder, int ExplicitOrder, ExportColumn Column)> buffer = new();

            for (int i = 0; i < properties.Length; i++)
            {
                PropertyInfo property = properties[i];
                if (!property.CanRead)
                {
                    continue;
                }

                ExportColumnAttribute? attribute = property.GetCustomAttribute<ExportColumnAttribute>();
                if (attribute?.Ignore == true)
                {
                    continue;
                }

                string header = string.IsNullOrWhiteSpace(attribute?.Header) ? property.Name : attribute!.Header!;
                ExportColumnType type = attribute?.Type ?? ExportColumnType.Text;

                Func<object, object?> selector = (row) => property.GetValue(row);

                buffer.Add((i, attribute?.Order ?? int.MaxValue, new ExportColumn(header, type, selector)));
            }

            return buffer
                .OrderBy(x => x.ExplicitOrder)
                .ThenBy(x => x.DeclarationOrder)
                .Select(x => x.Column)
                .ToList();
        }
        #endregion
    }
}
