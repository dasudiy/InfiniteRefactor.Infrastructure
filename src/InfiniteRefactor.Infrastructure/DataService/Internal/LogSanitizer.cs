namespace InfiniteRefactor.Infrastructure.DataService.Internal
{
    internal static class LogSanitizer
    {
        // Prevent log injection by escaping newline characters from user-controlled input
        internal static string Sanitize(string s) => s?.Replace("\r", "\\r").Replace("\n", "\\n");
    }
}
