using FluentFTP;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Storage
{
    public class XbinsStorageProvider : IStorageProvider
    {
        private CancellationTokenSource? _cancellationToken;

        private ILogger _logger;

        private FtpHelper? _ftpHelper;

        public XbinsStorageProvider(ILogger logger)
        {
            _logger = logger;
            _ftpHelper = null;
        }

        public bool IsReadOnly => true;

        public Protocol Protocol => Protocol.FTP;

        public async Task<bool> ConnectAsync()
        {
            _cancellationToken = new CancellationTokenSource();
            SoundPlayer.PlayFtpConnect();
            var ftpDetails = await XbinsHelper.ConnectIrcAsync(_logger, _cancellationToken);
            if (ftpDetails != null)
            {
                var ftpHelper = new FtpHelper();
                if (ftpHelper.Connect(ftpDetails, _logger, _cancellationToken))
                {
                    _ftpHelper = ftpHelper;
                }
            }
            return _ftpHelper != null;
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
            return false;
        }

        public bool TryDelete(FileItemInfo? fileItemInfo)
        {
            return false;
        }

        public bool TryRename(FileItemInfo? fileItemInfo, string new_name)
        {
            return false;
        }

        public Stream? OpenWriteStream(string path, long fileLen)
        {
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
