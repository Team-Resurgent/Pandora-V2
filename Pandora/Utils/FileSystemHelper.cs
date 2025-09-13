using Pandora.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;

namespace Pandora.Utils
{
    public static class FileSystemHelper
    {
        [DllImport("shell32.dll")]
        private static extern int SHGetKnownFolderPath([MarshalAs(UnmanagedType.LPStruct)] Guid rfid, uint dwFlags, IntPtr hToken, out IntPtr ppszPath);

        public static bool IsAccessDenied(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    using (var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        return false;
                    }
                }
                else if (Directory.Exists(path))
                {
                    var entries = Directory.GetFileSystemEntries(path);
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                // do nothing
            }
            return true;
        }
        
        public static IEnumerable<FileItemInfo> GetFileSystemEntries(string path)
        {
            var fileItems = new List<FileItemInfo>();

            var fses = Directory.EnumerateFileSystemEntries(path);
            foreach (var fse in fses)
            {
                try
                {
                    var attributes = File.GetAttributes(fse);
                    var hideItem = (attributes & (FileAttributes.Hidden | FileAttributes.System)) != 0;
                    if (hideItem)
                    {
                        continue;
                    }

                    var isDirectory = (attributes & FileAttributes.Directory) == FileAttributes.Directory;
                    var fileItem = new FileItemInfo
                    {
                        IsDirectory = isDirectory,
                        Name = Path.GetFileName(fse),
                        Path = fse,
                        Size = isDirectory ? 0 : new FileInfo(fse).Length
                    };
                    fileItems.Add(fileItem);
                }
                catch
                {
                    // skip files with error
                }
            }

            var result = fileItems.OrderByDescending(x => x.IsDirectory).ThenBy(x => x.Name).ToList();
            var parentDirectory = new FileInfo(path).Directory?.FullName;
            if (parentDirectory != null)
            {
                result.Insert(0, new FileItemInfo { IsDirectory = true, Name = "..", Path = parentDirectory, Size = 0 });
            }
            return result;
        }

        private static void AddDownloadsFolder(List<RootItemInfo> specialFolders)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Guid downloadsFolderGuid = new("374DE290-123F-4565-9164-39C4925E467B");
                IntPtr outPath;
                var result = SHGetKnownFolderPath(downloadsFolderGuid, 0, IntPtr.Zero, out outPath);
                if (result >= 0)
                {
                    string path = Marshal.PtrToStringUni(outPath)!;
                    Marshal.FreeCoTaskMem(outPath);
                    specialFolders.Add(new RootItemInfo { Name = "Downloads", Path = path });
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var configPath = Path.Combine(home, ".config", "user-dirs.dirs");

                if (File.Exists(configPath))
                {
                    foreach (var line in File.ReadAllLines(configPath))
                    {
                        if (line.StartsWith("XDG_DOWNLOAD_DIR"))
                        {
                            var value = line.Split('=')[1].Trim().Trim('"');
                            if (value.StartsWith("$HOME"))
                            {
                                specialFolders.Add(new RootItemInfo { Name = "Downloads", Path = value.Replace("$HOME", home) });
                            }
                            else
                            {
                                specialFolders.Add(new RootItemInfo { Name = "Downloads", Path = value });
                            }
                        }
                    }
                }

                var fallback = Path.Combine(home, "Downloads");
                if (Directory.Exists(fallback))
                {
                    specialFolders.Add(new RootItemInfo { Name = "Downloads", Path = fallback });
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                var downloads = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloads))
                {
                    specialFolders.Add(new RootItemInfo { Name = "Downloads", Path = downloads });
                }
            }
        }

        public static string? GetApplicationPath()
        {
            var exePath = AppDomain.CurrentDomain.BaseDirectory;
            if (exePath == null)
            {
                return null;
            }

            var result = Path.GetDirectoryName(exePath);
            return result;
        }

        public static IEnumerable<RootItemInfo> GetSpecialFolders()
        {
            var specialFolders = new List<RootItemInfo>();
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                specialFolders.Add(new RootItemInfo { Name = "/", Path = "/" });
                specialFolders.Add(new RootItemInfo { Name = "Desktop", Path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) });
                specialFolders.Add(new RootItemInfo { Name = "Documents", Path = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Documents" });
                AddDownloadsFolder(specialFolders);
                specialFolders.Add(new RootItemInfo { Name = "User", Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                specialFolders.Add(new RootItemInfo { Name = "Desktop", Path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) });
                specialFolders.Add(new RootItemInfo { Name = "Documents", Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) });
                AddDownloadsFolder(specialFolders);
                specialFolders.Add(new RootItemInfo { Name = "User", Path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) });
            }
          
            var logicalDrives = Directory.GetLogicalDrives();
            foreach (var logicalDrive in logicalDrives)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    if (logicalDrive.StartsWith("/Volume", StringComparison.CurrentCultureIgnoreCase))
                    {
                        specialFolders.Add(new RootItemInfo { Name = Path.GetFileName(logicalDrive), Path = logicalDrive });
                    }
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    specialFolders.Add(new RootItemInfo { Name = logicalDrive.Substring(0, 2), Path = logicalDrive });
                }
            }

            return specialFolders.ToArray();
        }

        public static bool IsValidFtpFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
            { 
                return false;
            }

            var reservedNames = new string[] { "CON", "PRN", "AUX", "NUL", "COM1", "COM2", "COM3", "COM4", "COM5", "COM6", "COM7", "COM8", "COM9", "LPT1", "LPT2", "LPT3", "LPT4", "LPT5", "LPT6", "LPT7", "LPT8", "LPT9" };
            if (Array.Exists(reservedNames, r => string.Equals(r, fileName, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }

            if (fileName.Length > 255)
            {
                return false;
            }

            string pattern = @"^(?![ .])[^\\/:*?""<>|\0-\x1F/]+(?<![ .])$";
            return Regex.IsMatch(fileName, pattern);
        }

        public static bool IsValidWindowsFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Reserved Windows names
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL",
                               "COM1","COM2","COM3","COM4","COM5","COM6","COM7","COM8","COM9",
                               "LPT1","LPT2","LPT3","LPT4","LPT5","LPT6","LPT7","LPT8","LPT9" };
            if (Array.Exists(reservedNames, r => string.Equals(r, fileName, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Regex for illegal characters and Windows quirks
            string pattern = @"^(?![ .])[^\\/:*?""<>|\0-\x1F]+(?<![ .])$";

            if (fileName.Length > 260) // Windows MAX_PATH limit
                return false;

            return Regex.IsMatch(fileName, pattern);
        }

        public static bool IsValidLinuxFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Disallow '/' (path separator) and null char
            string pattern = @"^[^/\0]+$";

            if (fileName.Length > 255) // typical Linux filesystem limit
                return false;

            return Regex.IsMatch(fileName, pattern);
        }

        public static bool IsValidFatXFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            // Reserved names
            string[] reservedNames = { "CON", "PRN", "AUX", "NUL" };

            if (Array.Exists(reservedNames, r => string.Equals(r, fileName, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Length limit
            if (fileName.Length > 42)
                return false;

            // Regex: allowed characters only
            string pattern = @"^[A-Za-z0-9\$%'\-_@~`!\(\)\{\}\^#&]+(\.[A-Za-z0-9\$%'\-_@~`!\(\)\{\}\^#&]+)?$";

            return Regex.IsMatch(fileName, pattern);
        }

        public static string CombinePathWin(string path, string value)
        {
            path = path.Replace("/", "\\").TrimEnd('\\');
            value = value.Replace("/", "\\").TrimStart('\\');
            return Path.Combine(path, value);
        }

        public static string CombinePathLinux(string path, string value)
        {
            path = path.Replace("\\", "/").TrimEnd('\\');
            value = value.Replace("\\", "/").TrimStart('\\');
            return string.Join("/", [path, value]).Replace("//", "/");
        }

        public static bool CopyStreamWithProgress(CancellationToken cancellationToken, Stream source, Stream destination, long expectedSize, Action<double> progress)
        {
            try
            {
                const int bufferSize = 81920; // 80 KB buffer
                byte[] buffer = new byte[bufferSize];
                long totalBytesCopied = 0;
                long totalLength = source.CanSeek ? source.Length : expectedSize;
                int bytesRead;

                // Reset source to beginning if possible
                if (source.CanSeek)
                {
                    source.Seek(0, SeekOrigin.Begin);
                }

                while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return false;
                    }
                    destination.Write(buffer, 0, bytesRead);
                    totalBytesCopied += bytesRead;

                    if (totalLength > 0)
                    {
                        progress.Invoke((double)totalBytesCopied / totalLength);
                    }
                }

                destination.Flush();
                return true;
            }
            catch
            {
            }
            return false;
        }
    }
}
