using Avalonia.Platform.Storage;
using Pandora.Logging;
using Pandora.Models;
using System.Collections.Generic;
using System.IO;
using Tmds.DBus.Protocol;

namespace Pandora.Storage
{
    public class ConnectionManager
    {
        private ILogger _logger;
        private Dictionary<IConnection, IStorageProvider> _storageProviders;

        public ConnectionManager(ILogger logger)
        {
            _logger = logger;
            _storageProviders = new Dictionary<IConnection, IStorageProvider>();
        }

        public IStorageProvider? GetStorageProvider(IConnection connection)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return null;
            }
            return _storageProviders[connection];
        }

        public bool Connect(IConnection connection)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                IStorageProvider? storageProvider = null;
                if (connection.ConnectionType == ConnectionType.Local)
                {
                    storageProvider = new LocalStorageProvider(_logger, connection);
                }
                else if (connection.ConnectionType == ConnectionType.FTP)
                {
                    var ftpConnection = connection as ConnectionFTP;
                    storageProvider = new FtpStorageProvider(_logger, connection) { FtpDetails = ftpConnection?.FtpDetails };
                }
                else if (connection.ConnectionType == ConnectionType.XBINS)
                {
                    storageProvider = new XbinsStorageProvider(_logger, connection);
                }

                if (storageProvider != null)
                {
                    _storageProviders.Add(connection, storageProvider);
                }
            }

            return _storageProviders[connection].Connect();
        }

        public void Disconnect(IConnection connection)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return;
            }
            _storageProviders[connection].Disconnect();
        }

        public bool TryGetRootItems(IConnection connection, out IEnumerable<RootItemInfo> rootItems)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                rootItems = [];
                return false;
            }
            return _storageProviders[connection].TryGetRootItems(out rootItems);
        }

        public bool TryGetFileItems(IConnection connection, string path, out IEnumerable<FileItemInfo> fileItems)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                fileItems = [];
                return false;
            }
            return _storageProviders[connection].TryGetFileItems(path, out fileItems);
        }

        public bool TryRecurseFileItems(IConnection connection, FileItemInfo folder, out IEnumerable<FileItemInfo> fileItems)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                fileItems = [];
                return false;
            }
            return _storageProviders[connection].TryRecurseFileItems(folder, out fileItems);
        }

        public bool CreateFolder(IConnection connection, string path)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return false;
            }
            return _storageProviders[connection].TryCreateFolder(path);
        }

        public bool Delete(IConnection connection, FileItemInfo? fileItemInfo)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return false;
            }
            return _storageProviders[connection].TryDelete(fileItemInfo);
        }

        public bool Rename(IConnection connection, FileItemInfo? fileItemInfo, string new_name)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return false;
            }
            return _storageProviders[connection].TryRename(fileItemInfo, new_name);
        }

        public Stream? OpenWriteStream(IConnection connection, string path, long fileLen)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return null;
            }
            return _storageProviders[connection].OpenWriteStream(path, fileLen);
        }

        public Stream? OpenReadStream(IConnection connection, string path, long fileLen)
        {
            if (!_storageProviders.ContainsKey(connection))
            {
                return null;
            }
            return _storageProviders[connection].OpenReadStream(path, fileLen);
        }
    }
}
