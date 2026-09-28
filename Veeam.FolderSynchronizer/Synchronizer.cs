using Veeam.FolderSynchronizer.FileComparison;
using Veeam.FolderSynchronizer.Logging;

namespace Veeam.FolderSynchronizer;

public enum FileComparisonApproach
{
    ByteByByte,
    Md5Hash
}
internal class Synchronizer
{
    private readonly DirectoryInfo _sourcePath;
    private readonly DirectoryInfo _replicaPath;
    private readonly int _timeIntervalSeconds;

    private readonly FileComparisonStrategy _fileComparisonStrategy;
    private readonly Logger _logger;

    public Synchronizer(string sourcePath, string replicaPath, int timeIntervalSeconds,
        FileComparisonApproach fileComparisonApproach, Logger logger)
    {
        _sourcePath = new DirectoryInfo(sourcePath);
        _replicaPath = new DirectoryInfo(replicaPath);
        _timeIntervalSeconds = timeIntervalSeconds;
        _logger = logger;

        _fileComparisonStrategy = fileComparisonApproach == FileComparisonApproach.ByteByByte
            ? new ByteByByteFileStreamsComparison(_logger)
            : new Md5HashFileComparison(_logger);

        _logger.LogEmptyLine();
        _logger.Log($"[INFO] File comparison approach: {fileComparisonApproach}");
    }

    public void Launch()
    {
        try
        {
            while (true)
            {
                SynchronizeFilesFoldersRecursively(_sourcePath, _replicaPath);
                Thread.Sleep(_timeIntervalSeconds * 1000);
            }
        }
        catch (Exception e)
        {
            _logger.Log($"[ERROR] {e}");
            throw;
        }
    }

    private void SynchronizeFilesFoldersRecursively(DirectoryInfo sourceFolder, DirectoryInfo replicaFolder)
    {
        _logger.LogEmptyLine();
        _logger.Log($"[INFO] Start validation in Source folder '{sourceFolder}', Replica folder '{replicaFolder}'");

        SynchronizeFilesOnOneLevel(sourceFolder, replicaFolder);

        SynchronizeDirectoriesOnOneLevel(sourceFolder, replicaFolder);

        sourceFolder.Refresh();
        replicaFolder.Refresh();
        var sourceDirectories = sourceFolder.GetDirectories();
        var replicaDirectories = replicaFolder.GetDirectories();

        foreach (var sourceDir in sourceDirectories)
        {
            var correspondentReplicaDir = replicaDirectories.First(d => d.Name == sourceDir.Name);
            SynchronizeFilesFoldersRecursively(sourceDir, correspondentReplicaDir);
        }
    }

    private void SynchronizeFilesOnOneLevel(DirectoryInfo sourceFolder, DirectoryInfo replicaFolder)
    {
        var sourceFiles = sourceFolder.GetFiles();
        var replicaFiles = replicaFolder.GetFiles();

        //Check if files present in source folder exist and are equal to files in replica folder
        foreach (var sourceFile in sourceFiles)
        {
            var replicaFile = replicaFiles.FirstOrDefault(f => f.Name == sourceFile.Name);

            if (replicaFile != null
                && _fileComparisonStrategy.AreFilesEqual(sourceFile.FullName, replicaFile.FullName)) continue;

            TryExecute(() => File.Copy(sourceFile.FullName, Path.Combine(replicaFolder.FullName, sourceFile.Name), true),
                $"Copied file '{sourceFile.FullName}' to replica folder '{replicaFolder.FullName}'",
                $"Couldn't copy '{sourceFile.FullName}' to replica folder '{replicaFolder.FullName}'. " +
                $"Possible reason: file is blocked by other operation");
        }

        //Delete files from replica folder that don't exist in source folder
        var filesToDelete = replicaFiles
            .Where(replica => sourceFiles.All(source => replica.Name != source.Name)).ToList();

        foreach (var fileToDelete in filesToDelete)
        {
            TryExecute(() => fileToDelete.Delete(),
                $"Deleted file '{fileToDelete.FullName}'",
                $"Couldn't delete '{fileToDelete.FullName}' from replica folder '{replicaFolder.FullName}'. " +
                $"Possible reason: file is blocked by other operation");
        }
    }

    private void SynchronizeDirectoriesOnOneLevel(DirectoryInfo sourceFolder, DirectoryInfo replicaFolder)
    {
        var sourceDirectories = sourceFolder.GetDirectories();
        var replicaDirectories = replicaFolder.GetDirectories();

        foreach (var sourceDir in sourceDirectories)
        {
            if (replicaDirectories.All(d => sourceDir.Name != d.Name))
            {
                replicaFolder.CreateSubdirectory(sourceDir.Name);
                _logger.Log($"Created '{sourceDir.Name}' directory in replica folder '{replicaFolder}'");
            }
        }

        var directoriesToDelete = replicaDirectories
            .Where(replicaDir => sourceDirectories.All(d => replicaDir.Name != d.Name)).ToList();

        foreach (var dirToDelete in directoriesToDelete)
        {
            TryExecute(() => dirToDelete.Delete(true),
                $"Deleted directory '{dirToDelete.Name}' from replica folder '{replicaFolder}'",
                $"Couldn't delete directory '{dirToDelete.FullName}' from replica folder '{replicaFolder.FullName}'. " +
                $"Possible reason: directory or files in is blocked by other operation");
        }
    }

    private void TryExecute(Action action, string positiveLog, string negativeLog)
    {
        try
        {
            action();
            _logger.Log($"[INFO] {positiveLog}");
        }
        catch (Exception e)
        {
            _logger.Log($"[ERROR] {negativeLog}");
            _logger.Log($"[DEBUG] Exception details: {e}");
        }
    }
}