using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace ZWebAPI.ExtensionMethods
{
    /// <summary>
    /// Extension methods for <see cref="Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry"/>.
    /// </summary>
    internal static class EntityEntryExtensions
    {
        /// <summary>
        /// Determines whether the value was modified.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        ///   <c>true</c> if the value was modified; otherwise, <c>false</c>.
        /// </returns>
        internal static bool HasValueModified(this PropertyEntry property)
        {
            return property.IsModified
            && ((property.OriginalValue != null && property.CurrentValue == null)
                || (property.OriginalValue == null && property.CurrentValue != null)
                || (property.OriginalValue != null && property.CurrentValue != null && !property.OriginalValue.Equals(property.CurrentValue))
            );
        }
    }
}