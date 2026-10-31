namespace ZWebAPI.Models.Audit.OperationHistory
{
    /// <summary>
    /// The kinds of value an <see cref="ZWebAPI.Models.Audit.OperationHistory.OperationsHistoryFieldModel"/> holds.
    /// </summary>
    public static class OperationsHistoryFieldTypes
    {
        /// <summary>
        /// A <see cref="bool"/>.
        /// </summary>
        public const string Boolean = "boolean";

        /// <summary>
        /// A date, with or without a time.
        /// </summary>
        public const string Date = "date";

        /// <summary>
        /// An enumeration, sent as its numeric value.
        /// </summary>
        public const string Enum = "enum";

        /// <summary>
        /// A primary or foreign key.
        /// </summary>
        public const string Key = "key";

        /// <summary>
        /// A number.
        /// </summary>
        public const string Number = "number";

        /// <summary>
        /// Anything else: binary data, a structured value.
        /// </summary>
        public const string Other = "other";

        /// <summary>
        /// A text.
        /// </summary>
        public const string Text = "text";

        /// <summary>
        /// A time of day or a duration.
        /// </summary>
        public const string Time = "time";
    }
}
