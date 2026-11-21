namespace ZWebAPI.Models
{
    /// <summary>
    /// Limits for paginated lists, shared by every list query.
    /// </summary>
    public static class ListPaging
    {
        #region Constants
        /// <summary>
        /// The default for <see cref="MaxPageSize"/>.
        /// </summary>
        public const int DefaultMaxPageSize = 1000;
        #endregion

        #region Variables
        private static int maxPageSize = DefaultMaxPageSize;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the most rows one list page returns. A request for more, or one with no
        /// end row, gets this many. Set it once at startup.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">When set to zero or less.</exception>
        public static int MaxPageSize
        {
            get => maxPageSize;
            set
            {
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(value);
                maxPageSize = value;
            }
        }
        #endregion
    }
}
