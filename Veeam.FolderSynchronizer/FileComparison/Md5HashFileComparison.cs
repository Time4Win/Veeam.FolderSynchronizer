using System.Diagnostics;
using System.Security.Cryptography;
using Veeam.FolderSynchronizer.Logging;

namespace Veeam.FolderSynchronizer.FileComparison;

internal class Md5HashFileComparison : FileComparisonStrategy
{
    private readonly Logger _logger;
    internal Md5HashFileComparison(Logger logger)
    {
        _logger = logger;
    }
    
    internal override bool AreFilesEqual(string filePath1, string filePath2)
    {
        if (!AreLengthsEqual(filePath1, filePath2))
        {
            return false;
        }

        var timer = new Stopwatch();
        timer.Start();

        var file1Info = new FileInfo(filePath1);
        using var fileStream1 = file1Info.OpenRead();
        using var fileStream2 = new FileInfo(filePath2).OpenRead();

        using var md5 = MD5.Create();

        var file1Hash = md5.ComputeHash(fileStream1);
        var file2Hash = md5.ComputeHash(fileStream2);

        var areEqual = file1Hash.SequenceEqual(file2Hash);
        
        timer.Stop();
        _logger.Log($"[INFO] Md5 Hash files comparison working time: {timer.Elapsed.TotalSeconds} second(s). File name: '{file1Info.Name}'");
        
        return areEqual;
    }
}