using Avalonia.Controls;
using FluentFTP;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Utils;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Storage
{
    public class FtpStorageProvider : IStorageProvider
    {
        private CancellationTokenSource? _cancellationToken;

        private ILogger _logger;

        private FtpHelper? _ftpHelper;

        public FtpDetails? FtpDetails { get; set; }

        public FtpStorageProvider(ILogger logger) 
        {
            _logger = logger;
        }

        public bool IsReadOnly => false;

        public Protocol Protocol => Protocol.FTP;

        public async Task<bool> ConnectAsync()
        {
            if (FtpDetails == null)
            {
                return await Task.FromResult(false);
            }
            SoundPlayer.PlayFtpConnect();
            _cancellationToken = new CancellationTokenSource();
            var ftpHelper = new FtpHelper();
            if (ftpHelper.Connect(FtpDetails, _logger, _cancellationToken))
            {
                _ftpHelper = ftpHelper;
            }
            return await Task.FromResult(_ftpHelper != null);
        }

        public void Disconnect()
        {
            _cancellationToken?.Cancel();
            _ftpHelper?.Disconnect();
            _ftpHelper = null;
            SoundPlayer.PlayFtpDisconnect();
        }

        public bool TryGetRootItems(out IEnumerable<RootItemInfo> rootItems)
        {
            if (_ftpHelper == null)
            {
                rootItems = [];
                return false;
            }
            return _ftpHelper.TryGetRootItems(out rootItems);
        }

        public bool TryGetFileItems(string path, out IEnumerable<FileItemInfo> fileItems)
        {
            if (_ftpHelper == null)
            {
                fileItems = [];
                return false;
            }
            return _ftpHelper.TryGetFileItems(path, out fileItems);
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
            if (_ftpHelper?.FtpClient == null)
            {
                return false;
            }
            try
            {
                _ftpHelper.FtpClient.CreateDirectory(path);
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
            if (_ftpHelper?.FtpClient == null)
            {
                return false;
            }
            if (fileItemInfo != null)
            {
                try
                {
                    if (fileItemInfo.IsDirectory)
                    {
                        _ftpHelper.FtpClient.DeleteDirectory(fileItemInfo.Path);
                    }
                    else
                    {
                        _ftpHelper.FtpClient.DeleteFile(fileItemInfo.Path);
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
            if (_ftpHelper?.FtpClient == null)
            {
                return false;
            }
            if (fileItemInfo != null)
            {
                try
                {
                    var dest = System.IO.Path.GetDirectoryName(fileItemInfo.Path)?.Replace("\\","/");
                    if (dest == null)
                    {
                        return false;
                    }
                    var target = FileSystemHelper.CombinePathLinux(dest, new_name);
                    if (fileItemInfo.IsDirectory)
                    {
                        _ftpHelper.FtpClient.MoveDirectory(fileItemInfo.Path, target);
                    }
                    else
                    {
                        _ftpHelper.FtpClient.MoveFile(fileItemInfo.Path, target);
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
            if (_ftpHelper?.FtpClient != null)
            {
                try
                {
                    return _ftpHelper.FtpClient.OpenWrite(path, FtpDataType.Binary, fileLen);
                }
                catch
                {
                    // do nothing
                }
            }
            return null;
        }

        public Stream? OpenReadStream(string path, long fileLen)
        {
            if (_ftpHelper?.FtpClient != null)
            {
                try
                {
                    return _ftpHelper.FtpClient.OpenRead(path, FtpDataType.Binary, 0, fileLen);
                }
                catch
                {
                    // do nothing
                }
            }
            return null;
        }
    }
}
