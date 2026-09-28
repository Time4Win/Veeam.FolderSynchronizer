namespace Veeam.FolderSynchronizer.Logging;

internal class ConsoleLogWriter : ILogWriter
{
    public void Log(string message)
    {
        Console.WriteLine(message);
    }
}