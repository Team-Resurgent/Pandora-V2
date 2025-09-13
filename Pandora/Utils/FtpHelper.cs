using FluentFTP;
using Pandora.Logging;
using Pandora.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Pandora.Utils
{
    public class FtpHelper
    {
        private FtpClient? _ftpClient;

        public FtpClient? FtpClient => _ftpClient;

        public bool Connect(FtpDetails ftpDetails, ILogger logger, CancellationTokenSource? cancellationToken)
        {
            try
            {
                _ftpClient = new FtpClient
                {
                    Credentials = new NetworkCredential(ftpDetails.User, ftpDetails.Password),
                    Host = ftpDetails.Host,
                    Port = ftpDetails.Port,
                };

                try
                {
                    _ftpClient.Connect();
                }
                catch (Exception ex)
                {
                    logger.LogMessage("Error", $"ConnectFTP: Unable to connect to FTP, disconnecting '{ex.Message}");
                    cancellationToken?.Cancel();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                logger.LogMessage("Error", $"ConnectFTP: Exception occured, disconnecting. '{ex.Message}'.");
                cancellationToken?.Cancel();
                return false;
            }
        }

        public void Disconnect()
        {
            _ftpClient?.Dispose();
        }

        public bool TryGetRootItems(out IEnumerable<RootItemInfo> rootItems)
        {
            if (_ftpClient != null)
            {
                try
                {
                    var tempRootItems = new List<RootItemInfo>();
                    var ftpListing = _ftpClient.GetListing("/", FtpListOption.AllFiles);

                    foreach (var item in ftpListing)
                    {
                        if (item.Type != FtpObjectType.Directory)
                        {
                            continue;
                        }

                        var rootItem = new RootItemInfo();
                        rootItem.Name = item.Name;
                        rootItem.Path = $"/{item.Name}";
                        rootItem.Selected = false;
                        tempRootItems.Add(rootItem);
                    }

                    var result = tempRootItems.OrderBy(s => s.Name).ToList();
                    result.Insert(0, new RootItemInfo { Name = "/", Path = "/", Selected = false });
                    rootItems = result;
                    return true;
                }
                catch
                {
                }
            }
            rootItems = [];
            return false;
        }

        public string? ParentFtpPath(string path)
        {
            if (path.Equals("/"))
            {
                return null;
            }
            var lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex > 0)
            {
                return path.Substring(0, lastSlashIndex);
            }
            return "/";
        }

        public bool TryGetFileItems(string path, out IEnumerable<FileItemInfo> fileItems)
        {
            if (_ftpClient != null)
            {
                try
                {
                    var tempFileItems = new List<FileItemInfo>();
                    var ftpListing = _ftpClient.GetListing(path, FtpListOption.AllFiles);

                    foreach (var item in ftpListing)
                    {
                        var fileItem = new FileItemInfo();
                        fileItem.IsDirectory = item.Type != FtpObjectType.File;
                        fileItem.Name = item.Name;
                        fileItem.Path = path.Equals("/") ? $"/{item.Name}" : $"{path}/{item.Name}";
                        fileItem.Size = item.Size;
                        fileItem.Selected = false;
                        tempFileItems.Add(fileItem);
                    }

                    var result = tempFileItems.OrderByDescending(x => x.IsDirectory).ThenBy(x => x.Name).ToList();
                    var parentDirectory = ParentFtpPath(path);
                    if (parentDirectory != null)
                    {
                        result.Insert(0, new FileItemInfo { IsDirectory = true, Name = "..", Path = parentDirectory, Size = 0 });
                    }
                    fileItems = result;
                    return true;
                }
                catch
                {
                }
            }
            fileItems = [];
            return false;
        }
    }
}
