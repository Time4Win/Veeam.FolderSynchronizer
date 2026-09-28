namespace Veeam.FolderSynchronizer.Logging;

internal interface ILogWriter
{
    internal void Log(string message);
}