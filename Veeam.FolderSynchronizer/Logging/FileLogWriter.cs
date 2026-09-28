namespace Veeam.FolderSynchronizer.Logging;

public class FileLogWriter : ILogWriter
{
    private readonly string _logFilePath;
    
    public FileLogWriter(string filePath)
    {
        _logFilePath = filePath;
    }

    public void Log(string message)
    {
        File.AppendAllText(_logFilePath, message + Environment.NewLine);
    }
}