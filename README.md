# Veeam.FolderSynchronizer

C# console application implementing one-way folder synchronization.

## Test Task Requirements & Implementation

The program maintains an identical copy of a **source** folder at a **replica** folder, satisfying the following specifications:

* **One-Way Synchronization**: After synchronization, the content of the replica folder is modified to match the content of the source folder (file creation, copying, and removal).
* **Periodic Execution**: Synchronization is performed at defined time intervals.
* **Dual Logging**: All file operations (creation, copying, removal) are recorded to the console output and written to a specified log file.
* **CLI Configuration**: Source path, replica path, synchronization interval, and log file path are provided via command-line arguments.
* **Implementation Constraint**: Developed in C# without third-party folder-synchronization libraries; custom logic is used for file comparison and synchronization.

## File Comparison Approaches

The project implements two approaches for comparing file content (see classes ByteByByteFileStreamsComparison.cs and Md5HashFileComparison.cs), 
both of which can be tested and evaluated. Comparison analysis of these 2 different approaches are documented in **`PerformanceAnalysis.md`**.

## Project Structure

* `Veeam.FolderSynchronizer/` - Core .NET 8 solution and source code files.
  * `FileComparison/` - File state and comparison engine.
  * `Logging/` - Logging mechanism for console and file outputs.
  * `Synchronizer.cs` - Core one-way synchronization logic.
  *  `Program.cs` - Entry point of the application and command-line arguments validation.
  * `PerformanceAnalysis.md` - Technical performance benchmarks and analysis.

## Parameters Description

The program requires exactly 4 arguments passed in a specific order:
1. `<SourceFolderPath>`: Path to the source folder being monitored.
2. `<ReplicaFolderPath>`: Path to the replica folder that will be synchronized.
3. `<IntervalInSeconds>`: Time interval between synchronization cycles, specified in **seconds** (e.g., `30`).
4. `<LogFilePath>`: Path to the file where all synchronization actions and file operations will be recorded. If the path to the log file does not exist, it will be created automatically.

## Example of Command Line

Run the application via the command line by providing the required arguments:

```bash
C:\source\repos\Veeam.FolderSynchronizer\Veeam.FolderSynchronizer\bin\Debug\net8.0\Veeam.FolderSynchronizer.exe c:\SourceFolder c:\ReplicaFolder 30 c:\Logs\Log01.log
