using Avalonia.Controls;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Utils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Pandora.Storage
{
    public class StorageProviderProxy : ReactiveObject
    {
        private IStorageProvider? _storageProvider;
        private ILogger _logger;

        public ObservableCollection<IConnection> ConnectionsAvailable { get; }

        private IConnection _selectedConnection;
        public IConnection SelectedConnection
        {
            get => _selectedConnection;
            set {
                CurrentPath = string.Empty;
                this.RaiseAndSetIfChanged(ref _selectedConnection, value);
            }
        }


        private string _currentPath;

        public string CurrentPath
        {
            get => _currentPath;
            set => this.RaiseAndSetIfChanged(ref _currentPath, value);
        }

        public ObservableCollection<RootItemInfo> Paths { get; } = [];

        private FileItemInfo? _currentFile;

        public FileItemInfo? CurrentFile
        {
            get => _currentFile;
            set => this.RaiseAndSetIfChanged(ref _currentFile, value);
        }

        public ObservableCollection<FileItemInfo> Files { get; } = [];

        public StorageProviderProxy(ILogger logger, ObservableCollection<IConnection> connectionsAvailable)
        {
            _storageProvider = null;
            _logger = logger;
            _selectedConnection = connectionsAvailable[0];
            _currentPath = string.Empty;

            ConnectionsAvailable = connectionsAvailable;
            NoActiveConnection = true;
            Connected = false;
            Disconnected = true;
        }

        public async Task ConnectAsync(IConnection connection, Window owner, Action<bool> onComplete)
        {
            Disconnect();

            NoActiveConnection = false;

            await Task.Run(async () =>
            {
                IStorageProvider? storageProvider = null;

                if (connection.ConnectionType == ConnectionType.Local)
                {
                    storageProvider = new LocalStorageProvider(_logger);
                }
                else if (connection.ConnectionType == ConnectionType.FTP)
                {
                    var ftpConnection = connection as ConnectionFTP;
                    storageProvider = new FtpStorageProvider(_logger) { FtpDetails = ftpConnection?.FtpDetails };
                }
                else if (connection.ConnectionType == ConnectionType.XBINS)
                {
                    storageProvider = new XbinsStorageProvider(_logger);
                }

                if (storageProvider != null)
                {
                    if (await storageProvider.ConnectAsync())
                    {
                        _storageProvider = storageProvider;
                    }
                }

                NoActiveConnection = true;
                Connected = _storageProvider != null;
                Disconnected = !Connected;

                onComplete(Connected);
            });
        }

        public bool IsReadOnly => _storageProvider?.IsReadOnly ?? true;

        public Protocol Protocol => _storageProvider?.Protocol ?? Protocol.Undefined;

        public void Disconnect()
        {
            if (_storageProvider == null)
            {
                return;
            }
            _storageProvider.Disconnect();
            _storageProvider = null;

            NoActiveConnection = true;
            Connected = false;
            Disconnected = true;
        }

        private bool _noActiveConnection;
        public bool NoActiveConnection
        {
            get => _noActiveConnection;
            set => this.RaiseAndSetIfChanged(ref _noActiveConnection, value);
        }

        private bool _connected;
        public bool Connected
        {
            get => _connected;
            set => this.RaiseAndSetIfChanged(ref _connected, value);
        }

        private bool _disconnected;
        public bool Disconnected
        {
            get => _disconnected;
            set => this.RaiseAndSetIfChanged(ref _disconnected, value);
        }

        public bool TryGetRootItems(out IEnumerable<RootItemInfo> rootItems)
        {
            if (_storageProvider == null)
            {
                rootItems = [];
                return false;
            }
            return _storageProvider.TryGetRootItems(out rootItems);
        }

        public bool TryGetFileItems(string path, out IEnumerable<FileItemInfo> fileItems)
        {
            if (_storageProvider == null)
            {
                fileItems = [];
                return false;
            }
            return _storageProvider.TryGetFileItems(path, out fileItems);
        }

        public bool TryRecurseFileItems(FileItemInfo folder, out IEnumerable<FileItemInfo> fileItems)
        {
            if (_storageProvider == null)
            {
                fileItems = [];
                return false;
            }
            return _storageProvider.TryRecurseFileItems(folder, out fileItems);
        }

        public string CombinePath(string path, string value)
        {
            if (Protocol == Protocol.FTP)
            {
                return FileSystemHelper.CombinePathLinux(path, value);
            }
            return FileSystemHelper.CombinePathWin(path, value);
        }

        public bool CreateFolder(string path)
        {
            if (_storageProvider == null)
            {
                return false;
            }
            return _storageProvider.TryCreateFolder(path);
        }

        public bool Delete(FileItemInfo? fileItemInfo)
        {
            if (_storageProvider == null)
            {
                return false;
            }
            return _storageProvider.TryDelete(fileItemInfo);
        }

        public bool Rename(FileItemInfo? fileItemInfo, string new_name)
        {
            if (_storageProvider == null)
            {
                return false;
            }
            return _storageProvider.TryRename(fileItemInfo, new_name);
        }

        public Stream? OpenWriteStream(string path, long fileLen)
        {
            if (_storageProvider != null)
            {
                return _storageProvider.OpenWriteStream(path, fileLen);
            }
            return null;
        }

        public Stream? OpenReadStream(string path, long fileLen)
        {
            if (_storageProvider != null)
            {
                return _storageProvider.OpenReadStream(path, fileLen);
            }
            return null;
        }
    }
}
