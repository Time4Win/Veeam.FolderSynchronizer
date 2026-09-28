using System.Diagnostics;
using Veeam.FolderSynchronizer.Logging;

namespace Veeam.FolderSynchronizer.FileComparison;

internal class ByteByByteFileStreamsComparison : FileComparisonStrategy
{
    private readonly Logger _logger;
    internal ByteByByteFileStreamsComparison(Logger logger)
    {
        _logger = logger;
    }
    private const int BufferSize = 8192; // 8K

    internal override bool AreFilesEqual(string filePath1, string filePath2)
    {
        if (!AreLengthsEqual(filePath1, filePath2))
        {
            return false;
        }

        var timer = new Stopwatch();
        
        timer.Start();
        var areEqual = CompareByBytesBuffers(filePath1, filePath2);
        timer.Stop();

        var file1Info = new FileInfo(filePath1);
        _logger.Log($"[INFO] Buffer byte by byte files comparison working time: {timer.Elapsed.TotalSeconds} second(s). File name: '{file1Info.Name}'");
        return areEqual;
    }

    private static bool CompareByBytesBuffers(string filePath1, string filePath2)
    {
        using var fs1 = new FileStream(filePath1, FileMode.Open, FileAccess.Read);
        using var fs2 = new FileStream(filePath2, FileMode.Open, FileAccess.Read);

        //Reading bytes by small portions prevents loading large data into RAM
        var buffer1 = new byte[BufferSize];
        var buffer2 = new byte[BufferSize];

        int bytesCount1;

        while ((bytesCount1 = fs1.Read(buffer1, 0, buffer1.Length)) > 0)
        {
            var bytesCount2 = fs2.Read(buffer2, 0, buffer2.Length);

            //additional size verification in case when files gets modified while it is being reading 
            if (bytesCount1 != bytesCount2)
            {
               return false;
            }

            for (var i = 0; i < bytesCount1; i++)
            {
                if (buffer1[i] != buffer2[i])
                    return false;
            }
        }

        return true;
    }
}