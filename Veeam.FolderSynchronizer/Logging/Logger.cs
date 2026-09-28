using System.Globalization;

namespace Veeam.FolderSynchronizer.Logging;

internal class Logger
{
    private readonly List<ILogWriter> _logWriters;
    internal Logger(List<ILogWriter> logWriters)
    {
        _logWriters = logWriters;
    }

    internal void Log(string message)
    {
        var timeStamp = DateTime.Now;
        foreach (var logWriter in _logWriters)
        {
            logWriter.Log($"{timeStamp.ToString(CultureInfo.InvariantCulture)} {message}");
        }
    }

    internal void LogEmptyLine()
    {
        foreach (var logWriter in _logWriters)
        {
            logWriter.Log("");
        }
    }
}