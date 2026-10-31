using System.Text.Json;

namespace ZWebAPI.Models.Audit.OperationHistory
{
    /// <summary>
    /// One field of an audited record, as it was before and after an operation.
    /// </summary>
    public class OperationsHistoryFieldModel
    {
        /// <summary>
        /// Gets or sets a value indicating whether the operation changed this field.
        /// </summary>
        /// <value>
        ///   <c>true</c> when the operation changed the field; otherwise, <c>false</c>.
        /// </value>
        public bool IsChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the field is bookkeeping rather than data: the
        /// key, the audit columns, the concurrency token.
        /// </summary>
        /// <value>
        ///   <c>true</c> when the field belongs to the framework; otherwise, <c>false</c>.
        /// </value>
        public bool IsSystem { get; set; }

        /// <summary>
        /// Gets or sets the property name.
        /// </summary>
        /// <value>
        /// The property name.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the value after the operation. <c>null</c> for a deletion, or when the
        /// value is not known.
        /// </summary>
        /// <value>
        /// The new value.
        /// </value>
        public JsonElement? NewValue { get; set; }

        /// <summary>
        /// Gets or sets the value before the operation. <c>null</c> for a creation, or when the
        /// value is not known.
        /// </summary>
        /// <value>
        /// The old value.
        /// </value>
        public JsonElement? OldValue { get; set; }

        /// <summary>
        /// Gets or sets what kind of value the field holds, so a client can format it.
        /// </summary>
        /// <value>
        /// One of the <see cref="ZWebAPI.Models.Audit.OperationHistory.OperationsHistoryFieldTypes"/> values.
        /// </value>
        public string Type { get; set; } = OperationsHistoryFieldTypes.Other;
    }
}
