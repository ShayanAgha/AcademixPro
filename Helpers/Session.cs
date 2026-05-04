namespace AcademixPro.Helpers
{
    /// <summary>
    /// Stores the currently logged-in user's info for the session.
    /// </summary>
    public static class Session
    {
        public static int    UserID   { get; set; }
        public static string Username { get; set; } = "";
        public static string FullName { get; set; } = "";
        public static string Email    { get; set; } = "";
        public static string Role     { get; set; } = "";

        public static void Clear()
        {
            UserID = 0;
            Username = FullName = Email = Role = "";
        }
    }
}
