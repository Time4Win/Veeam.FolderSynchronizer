using System.Text;
using Veeam.FolderSynchronizer.Logging;

namespace Veeam.FolderSynchronizer;

internal class Program
{
    private static readonly StringBuilder InputArgumentsErrors = new();
    private static void Main(string[] args)
    {
        InputArgumentsErrors.Clear();

        if (args.Length < 4)
        {
            Console.WriteLine("[ERROR] Please provide 4 arguments: <SourceFolderPath> <ReplicaFolderPath> <IntervalInSeconds> <LogFilePath>");
            return;
        }

        var pathSource = args[0];
        var pathReplica = args[1];
        var timeIntervalStr = args[2];
        var logFilePath = args[3];

        CheckFolderArg(pathSource, "<SourceFolderPath>");
        CheckFolderArg(pathReplica, "<ReplicaFolderPath>");
        CheckTimeInterval(timeIntervalStr, out var timeInterval);
        CheckCreateDirectoryAndLogFile(logFilePath);

        if (pathSource == pathReplica)
        {
            InputArgumentsErrors.AppendLine("[ERROR] Source and Replica folders are teh same");
        }

        if (InputArgumentsErrors.Length > 0)
        {
            Console.WriteLine(InputArgumentsErrors.ToString());
            return;
        }

        //one more safety moment - check if source folder and replica folder are not the same folder
        if (string.Equals(Path.GetFullPath(pathSource), Path.GetFullPath(pathReplica), StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("[ERROR] Source and Replica could not be the same folder.");
            return;
        }

        //Choose one of file comparison approach, see PerformanceAnalysis.md for test results info about both approaches
        const FileComparisonApproach comparisonStrategy = FileComparisonApproach.ByteByByte;

        var logger = new Logger([new ConsoleLogWriter(), new FileLogWriter(logFilePath)]);

        var synchronizer = new Synchronizer(pathSource, pathReplica, timeInterval, comparisonStrategy, logger);

        synchronizer.Launch();
    }

    private static void CheckFolderArg(string folderPath, string argType)
    {
        if (!Directory.Exists(folderPath))
        {
            InputArgumentsErrors.AppendLine($"[ERROR] {argType} folder '{folderPath}' is invalid directory path");
        }
    }

    private static void CheckTimeInterval(string timeIntervalStr, out int timeInterval)
    {

        if (!int.TryParse(timeIntervalStr, out timeInterval))
        {
            InputArgumentsErrors.AppendLine("[ERROR] Invalid argument <IntervalInSeconds>");
        }
    }

    private static void CheckCreateDirectoryAndLogFile(string filePath)
    {
        try
        {
            var dir = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dir!);

            //Create file if it doesn't exist for validation and to avoid rewriting it
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }
        }
        catch (Exception e)
        {
            InputArgumentsErrors.AppendLine($"[ERROR] Path '{filePath}'to log file is invalid");
            InputArgumentsErrors.AppendLine($"[DEBUG] {e}");
        }
    }
}