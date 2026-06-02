using System.Collections.Generic;
using System.Linq;

namespace ZWebAPI.Exporters
{
    /// <summary>
    /// Envelope returned by list endpoints that opt into the "total rows" contract.
    /// Carries the paginated <see cref="Items"/> and the <see cref="TotalRows"/> matching
    /// the current filter (pre-pagination).
    /// </summary>
    /// <typeparam name="T">Row model type.</typeparam>
    public sealed class ListResult<T>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ListResult{T}"/> class.
        /// </summary>
        /// <param name="items">Paginated items to return.</param>
        /// <param name="totalRows">Total rows matching the filter before pagination.</param>
        public ListResult(IEnumerable<T> items, int totalRows)
        {
            Items = items;
            TotalRows = totalRows;
        }
        #endregion

        #region Properties
        /// <summary>Paginated items.</summary>
        public IEnumerable<T> Items { get; }

        /// <summary>Total rows matching the filter, before pagination.</summary>
        public int TotalRows { get; }
        #endregion

        #region Public methods
        /// <summary>
        /// Convenience factory that materializes <paramref name="items"/> and captures the total.
        /// </summary>
        public static ListResult<T> From(IEnumerable<T> items, int totalRows)
        {
            return new ListResult<T>(items is List<T> ? items : items.ToList(), totalRows);
        }
        #endregion
    }
}
