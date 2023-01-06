using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.Json;

namespace FileSystemCheckerClassLib
{
    public class FileSystemChecker
    {
        public FileSystemChecker(bool isFromTestCase)
        {
            using var watcher = new FileSystemWatcher(@"C:\Users\Admin\Documents\Testing");

            watcher.NotifyFilter = NotifyFilters.Attributes
                                 | NotifyFilters.CreationTime
                                 | NotifyFilters.DirectoryName
                                 | NotifyFilters.FileName
                                 | NotifyFilters.LastAccess
                                 | NotifyFilters.LastWrite
                                 | NotifyFilters.Security
                                 | NotifyFilters.Size;

            watcher.Changed += new System.IO.FileSystemEventHandler(OnChanged);
            watcher.Created += new System.IO.FileSystemEventHandler(OnCreated);
            watcher.Deleted += new System.IO.FileSystemEventHandler(OnDeleted);
            watcher.Renamed += new System.IO.RenamedEventHandler(OnRenamed);
            watcher.Error += new System.IO.ErrorEventHandler(OnError);

            watcher.Filter = "*.*";
            watcher.IncludeSubdirectories = true;
            watcher.EnableRaisingEvents = true;

            if (!isFromTestCase)
            {
                CheckFilesChangedInLastHour();

                Console.WriteLine("Press enter to exit.");
                Console.ReadLine();
            }
        }


        public bool CheckFilesChangedInLastHour()
        {
            try
            {
                var watch = System.Diagnostics.Stopwatch.StartNew();

                List<ProcessedFilesData> _data = new List<ProcessedFilesData>();
                string path = @"C:\Users\Admin\Documents\Testing";
                string[] folders = Directory.GetDirectories(path, "*.*", SearchOption.AllDirectories);

                string[] files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    Console.WriteLine("Files changed in the last hour (if any):");
                    Console.WriteLine();

                    int fileCount = 0;
                    int folderCount = 0;
                    string folderNameToProcess = string.Empty;

                    foreach (string file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (folderNameToProcess != fileInfo.Directory?.Name)
                        {
                            folderCount++;
                        }
                        folderNameToProcess = fileInfo.Directory?.Name;

                        if (fileInfo != null && fileInfo.Length > 0)
                        {
                            DateTime lastWriteTime = fileInfo.LastWriteTime;
                            int duration = DateTime.Now.Hour - lastWriteTime.Hour;
                            if (duration == 1)
                            {
                                //Save the Processed File Information in JSON object and then Save the File

                                _data.Add(new ProcessedFilesData()
                                {
                                    Id = Guid.NewGuid(),
                                    FileName = fileInfo.Name,
                                    FilePath = fileInfo.FullName
                                });

                                if(File.Exists(path + @"\ProcessedFiles.json"))
                                {
                                    File.Delete(path + @"\ProcessedFiles.json");
                                }

                                using FileStream createStream = File.Create(path + @"\ProcessedFiles.json");
                                JsonSerializer.SerializeAsync(createStream, _data);

                                //List the FileName
                                Console.WriteLine($"FileName: {file}");
                                Console.WriteLine();
                                fileCount++;
                            }
                        }
                    }

                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;

                    Console.WriteLine($"Total Execution Time in Milliseconds: {elapsedMs}");
                    Console.WriteLine();
                    if (elapsedMs > 60000)
                    {
                        int executionMinutes = Convert.ToInt32(elapsedMs) / 60000;
                        Console.WriteLine($"Total Execution Time in Minutes: {elapsedMs}");
                        Console.WriteLine();
                    }

                    Console.WriteLine($"Total number of folders processed: {folderCount}");
                    Console.WriteLine();
                    Console.WriteLine($"Total number of files processed: {fileCount}");

                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }

        }

        private static void OnChanged(object sender, FileSystemEventArgs e)
        {
            if (!IsFileReady(e.FullPath)) return;

            if (e.ChangeType != WatcherChangeTypes.Changed)
            {
                return;
            }
            Console.WriteLine($"Changed: {e.FullPath}");
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            string value = $"Created: {e.FullPath}";
            Console.WriteLine(value);
        }

        private static void OnDeleted(object sender, FileSystemEventArgs e) =>
            Console.WriteLine($"Deleted: {e.FullPath}");

        private static void OnRenamed(object sender, RenamedEventArgs e)
        {
            Console.WriteLine($"Renamed:");
            Console.WriteLine($"    Old: {e.OldFullPath}");
            Console.WriteLine($"    New: {e.FullPath}");
        }

        private static void OnError(object sender, ErrorEventArgs e) =>
            PrintException(e.GetException());

        private static void PrintException(Exception? ex)
        {
            if (ex != null)
            {
                Console.WriteLine($"Message: {ex.Message}");
                Console.WriteLine("Stacktrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine();
                PrintException(ex.InnerException);
            }
        }
        private static bool IsFileReady(string path)
        {
            try
            {
                using (var file = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    return true;
                }
            }
            catch (IOException)
            {
                return false;
            }
        }
    }
}
