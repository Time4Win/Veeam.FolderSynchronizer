namespace Veeam.FolderSynchronizer.FileComparison
{
    internal abstract class FileComparisonStrategy
    {
        internal abstract bool AreFilesEqual(string filePath1, string filePath2);

        protected static bool AreLengthsEqual(string filePath1, string filePath2)
        {
            return new FileInfo(filePath1).Length == new FileInfo(filePath2).Length;
        }
    }
}
