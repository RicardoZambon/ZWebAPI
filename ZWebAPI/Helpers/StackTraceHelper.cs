using System.Diagnostics;
using System.Reflection;
using ZWebAPI.Attributes;

namespace ZWebAPI.Helpers
{
    /// <summary>
    /// Helper static class for <see cref="ZWebAPI.Attributes.ActionMethodAttribute"/>.
    /// </summary>
    public static class StackTraceHelper
    {
        /// <summary>
        /// Gets the method that implements the <see cref="ZWebAPI.Attributes.ActionMethodAttribute"/>.
        /// </summary>
        /// <param name="ignoreName">Name of the ignore.</param>
        /// <returns>If found, the method that implements the <see cref="ZWebAPI.Attributes.ActionMethodAttribute"/>; otherwise, <c>null</c>.</returns>
        public static MethodBase? GetMethodImplementingActionMethodAttribute()
        {
            StackTrace stackTrace = new();

            foreach (StackFrame stackFrame in stackTrace.GetFrames())
            {
                MethodBase? method = stackFrame.GetMethod();

                if (method?.GetCustomAttribute<ActionMethodAttribute>() is not null)
                {
                    return method;
                }
            }
            return null;
        }
    }
}