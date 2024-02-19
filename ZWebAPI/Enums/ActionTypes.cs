namespace ZWebAPI.Enums
{
    /// <summary>
    /// Enum for  user permissions in action types.
    /// </summary>
    public enum ActionTypes
    {
        /// <summary>
        /// Only regular users and administrators.
        /// </summary>
        RegularUsersAndAdmins = 0,
        /// <summary>
        /// Only administrators.
        /// </summary>
        OnlyAdmins = 1,
        /// <summary>
        /// Only regular users.
        /// </summary>
        OnlyRegularUsers = 2,
    }
}