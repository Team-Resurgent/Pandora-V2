using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Platform.Storage;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Utils;
using Pandora.ViewModels;
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
        public ObservableCollection<IConnection> ConnectionsAvailable { get; }

        private ConnectionManager _connectionManager;

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

        public StorageProviderProxy(ILogger logger, ObservableCollection<IConnection> connectionsAvailable, ConnectionManager connectionManager)
        {
            _selectedConnection = connectionsAvailable[0];
            _currentPath = string.Empty;

            _connectionManager = connectionManager;
            ConnectionsAvailable = connectionsAvailable;
            NoActiveConnection = true;
            Connected = false;
            Disconnected = true;
        }

        public bool Connect()
        {
            Disconnect();

            NoActiveConnection = false;

            var connected = _connectionManager.Connect(_selectedConnection);

            NoActiveConnection = true;
            Connected = connected;
            Disconnected = !connected;
            return connected;
        }

        public async Task ConnectAsync(Action<bool> onComplete)
        {
            Disconnect();

            NoActiveConnection = false;

            await Task.Run(() =>
            {
                var connected = _connectionManager.Connect(_selectedConnection);

                NoActiveConnection = true;
                Connected = connected;
                Disconnected = !connected;

                onComplete(connected);
            });
        }

        public bool IsReadOnly => _connectionManager.GetStorageProvider(_selectedConnection)?.IsReadOnly ?? true;

        public Protocol Protocol => _connectionManager.GetStorageProvider(_selectedConnection)?.Protocol ?? Protocol.Undefined;

        public void Disconnect()
        {
            _connectionManager.Disconnect(_selectedConnection);
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

        public string CombinePath(string path, string value)
        {
            if (Protocol == Protocol.FTP)
            {
                return FileSystemHelper.CombinePathLinux(path, value);
            }
            return FileSystemHelper.CombinePathWin(path, value);
        }


        public IStorageProvider? GetStorageProvider(IConnection connection)
        {
            return _connectionManager.GetStorageProvider(connection);
        }

        public bool Connect(IConnection connection)
        {
            return _connectionManager.Connect(connection);
        }

        public void Disconnect(IConnection connection)
        {
            _connectionManager.Disconnect(connection);
        }

        public bool TryGetRootItems(IConnection connection, out IEnumerable<RootItemInfo> rootItems)
        {
            return _connectionManager.TryGetRootItems(connection, out rootItems);
        }

        public bool TryGetFileItems(IConnection connection, string path, out IEnumerable<FileItemInfo> fileItems)
        {
            return _connectionManager.TryGetFileItems(connection, path, out fileItems);
        }

        public bool TryRecurseFileItems(IConnection connection, FileItemInfo folder, out IEnumerable<FileItemInfo> fileItems)
        {
            return _connectionManager.TryRecurseFileItems(connection, folder, out fileItems);
        }

        public bool CreateFolder(IConnection connection, string path)
        {
            return _connectionManager.CreateFolder(connection, path);
        }

        public bool Delete(IConnection connection, FileItemInfo? fileItemInfo)
        {
            return _connectionManager.Delete(connection, fileItemInfo);
        }

        public bool Rename(IConnection connection, FileItemInfo? fileItemInfo, string new_name)
        {
            return _connectionManager.Rename(connection, fileItemInfo, new_name);
        }

        public Stream? OpenWriteStream(IConnection connection, string path, long fileLen)
        {
            return _connectionManager.OpenWriteStream(connection, path, fileLen);
        }

        public Stream? OpenReadStream(IConnection connection, string path, long fileLen)
        {
            return _connectionManager.OpenReadStream(connection, path, fileLen);
        }
    }
}
