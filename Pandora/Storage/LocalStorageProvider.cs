using Pandora.Logging;
using Pandora.Models;
using Pandora.Utils;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;

namespace Pandora.Storage
{
    public class LocalStorageProvider : IStorageProvider
    {
        private ILogger _logger;
        private IConnection _connection;

        public LocalStorageProvider(ILogger logger, IConnection connection)
        {
            _logger = logger;
            _connection = connection;
        }

        public bool IsReadOnly => false;

        public Protocol Protocol => Protocol.Local;

        public IConnection Connection => _connection;

        public bool Connect()
        {
            SoundPlayer.PlayDriveConnect();
            return true;
        }

        public void Disconnect()
        {
            SoundPlayer.PlayDriveDisconnect();
        }

        public bool TryGetRootItems(out IEnumerable<RootItemInfo> rootItems)
        {
            rootItems = FileSystemHelper.GetSpecialFolders();
            return true;
        }

        public bool TryGetFileItems(string path, out IEnumerable<FileItemInfo> fileItems)
        {
            fileItems = FileSystemHelper.GetFileSystemEntries(path);
            return true;
        }

        public bool TryRecurseFileItems(FileItemInfo folder, out IEnumerable<FileItemInfo> fileItems)
        {
            var result = new List<FileItemInfo>();
            var stack = new Stack<string>();

            result.Add(folder);
            stack.Push(folder.Path);

            while (stack.Count > 0)
            {
                var currentPath = stack.Pop();
                if (!TryGetFileItems(currentPath, out var items))
                {
                    fileItems = [];
                    return false;
                }
                foreach (var item in items)
                {
                    if (item.Name.Equals(".."))
                    {
                        continue;
                    }
                    result.Add(item);
                    if (item.IsDirectory)
                    {
                        stack.Push(item.Path);
                    }
                }
            }
            fileItems = result;
            return true;
        }

        public bool TryCreateFolder(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                return true;
            }
            catch 
            {
                // do nothing
            }
            return false;
        }

        public bool TryDelete(FileItemInfo? fileItemInfo)
        {
            if (fileItemInfo != null)
            {
                try
                {
                    if (fileItemInfo.IsDirectory)
                    {
                        Directory.Delete(fileItemInfo.Path, true);
                    }
                    else
                    {
                        File.Delete(fileItemInfo.Path);
                    }
                    return true;
                }
                catch
                {
                    // do nothing
                }
            }
            return false;
        }

        public bool TryRename(FileItemInfo? fileItemInfo, string new_name)
        {
            if (fileItemInfo != null)
            {
                try
                {
                    var dest = Path.GetDirectoryName(fileItemInfo.Path);
                    if (dest == null)
                    {
                        return false;
                    }
                    var target = Path.Combine(dest, new_name);
                    if (fileItemInfo.IsDirectory)
                    {
                        Directory.Move(fileItemInfo.Path, target);
                    }
                    else
                    {
                        File.Move(fileItemInfo.Path, target);
                    }
                    return true;
                }
                catch
                {
                    // do nothing
                }
            }
            return false;
        }

        public Stream? OpenWriteStream(string path, long fileLen)
        {
            try
            {
                return new FileStream(path, FileMode.Create, FileAccess.Write);
            }
            catch
            {
                // do nothing
            }
            return null;
        }

        public Stream? OpenReadStream(string path, long fileLen)
        {
            try
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                // do nothing
            }
            return null;
        }
    }
}
