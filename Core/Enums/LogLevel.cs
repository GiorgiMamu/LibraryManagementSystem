namespace Core.Enums
{
    // severity levels for logging messages in the system
    public enum LogLevel
    {
        Info, // general information about the system's operation
        Warning, // something unexpected happened, but the system can continue to operate
        Error // something went wrong, and the system may not be able to continue to operate
    }
}