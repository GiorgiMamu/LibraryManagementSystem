using System;

namespace Core.Interfaces
{
    // contract for logging. Defined here so Services can log
    // ("book approved", "error saving file") without knowing
    // whether logs go to a text file, console, or somewhere else
    public interface ILogger
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception ex = null);
    }
}