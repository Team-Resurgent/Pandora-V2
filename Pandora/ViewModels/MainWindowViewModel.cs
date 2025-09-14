using Avalonia.Controls;
using Avalonia.Diagnostics;
using Avalonia.Threading;
using DynamicData;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Permissions;
using Pandora.Storage;
using Pandora.Utils;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        public Window? Owner { get; set; }

        public ICommand ExitCommand { get; }

        public ICommand FTPManageCommand { get; }

        public ICommand SourceConnectCommand { get; }

        public ICommand SourceCreateFolderCommand { get; }

        public ICommand SourceRenameCommand { get; }

        public ICommand SourceDeleteCommand { get; }

        public ICommand SourceCopyCommand { get; }

        public ICommand DestConnectCommand { get; }

        public ICommand DestCreateFolderCommand { get; }

        public ICommand DestRenameCommand { get; }

        public ICommand DestDeleteCommand { get; }

        public ICommand DestCopyCommand { get; }

        public ICommand ClearQueueCommand { get; }

        public ICommand ClearCompletedCommand { get; }

        public ICommand RetryFailedCommand { get; }

        public ICommand UnpauseCommand { get; }

        public FileContextPermissions SourceFileContextPermissions { get; } = new FileContextPermissions();

        public FileContextPermissions DestFileContextPermissions { get; } = new FileContextPermissions();

        public StorageProviderProxy SourceStorageProviderProxy { get; }

        public StorageProviderProxy DestStorageProviderProxy { get; }

        public DownloadQueueProcessor DownloadQueueProcessor { get; }

        public void OnSourcePathDoubleClicked(Control sender, RootItemInfo item)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!SourceStorageProviderProxy.TryGetFileItems(SourceStorageProviderProxy.SelectedConnection, item.Path, out var fileItems))
                {
                    Disconnect(SourceStorageProviderProxy);
                    return;
                }

                SourceFileContextPermissions.ClearPermissions();

                SourceStorageProviderProxy.CurrentPath = item.Path;
                SourceStorageProviderProxy.CurrentFile = null;
                SourceStorageProviderProxy.Files.Clear();
                SourceStorageProviderProxy.Files.AddRange(fileItems);
            });
        }

        public void OnSourceFileDoubleClicked(Control sender, FileItemInfo item)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (item.IsDirectory)
                {
                    if (!SourceStorageProviderProxy.TryGetFileItems(SourceStorageProviderProxy.SelectedConnection, item.Path, out var fileItems))
                    {
                        Disconnect(SourceStorageProviderProxy);
                        return;
                    }

                    SourceFileContextPermissions.ClearPermissions();

                    SourceStorageProviderProxy.CurrentPath = item.Path;
                    SourceStorageProviderProxy.CurrentFile = null;
                    SourceStorageProviderProxy.Files.Clear();
                    SourceStorageProviderProxy.Files.AddRange(fileItems);
                }
            });
        }

        public void OnSourcePathClicked(Control sender, RootItemInfo item)
        {
            foreach (var path in SourceStorageProviderProxy.Paths)
            {
                path.Selected = false;
            }
            item.Selected = true;
        }

        public void OnSourceFileClicked(object sender, FileItemInfo item)
        {
            SourceStorageProviderProxy.CurrentFile = item;
            foreach (var file in SourceStorageProviderProxy.Files)
            {
                file.Selected = false;
            }
            item.Selected = true;
        }

        public void OnSourceContextMenuOpening(object? sender, CancelEventArgs e)
        {
            var isWritableFrom = !SourceStorageProviderProxy.IsReadOnly;
            var isWritableTarget = !DestStorageProviderProxy.IsReadOnly;
            var isFileOrDir = !SourceStorageProviderProxy.CurrentFile?.Name.Equals("..") ?? true;
            SourceFileContextPermissions.CanCreateFolder = isWritableFrom;
            SourceFileContextPermissions.CanRename = isWritableFrom && isFileOrDir;
            SourceFileContextPermissions.CanDelete = isWritableFrom && isFileOrDir;
            SourceFileContextPermissions.CanCopyToTarget = isWritableTarget && isFileOrDir;
        }

        public void OnDestPathDoubleClicked(Control sender, RootItemInfo item)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!DestStorageProviderProxy.TryGetFileItems(DestStorageProviderProxy.SelectedConnection, item.Path, out var fileItems))
                {
                    Disconnect(DestStorageProviderProxy);
                    return;
                }

                DestFileContextPermissions.ClearPermissions();

                DestStorageProviderProxy.CurrentFile = null;
                DestStorageProviderProxy.CurrentPath = item.Path;
                DestStorageProviderProxy.Files.Clear();
                DestStorageProviderProxy.Files.AddRange(fileItems);
            });
        }

        public void OnDestFileDoubleClicked(object sender, FileItemInfo item)
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (item.IsDirectory)
                {
                    if (!DestStorageProviderProxy.TryGetFileItems(DestStorageProviderProxy.SelectedConnection, item.Path, out var fileItems))
                    {
                        Disconnect(DestStorageProviderProxy);
                        return;
                    }

                    DestFileContextPermissions.ClearPermissions();
                    DestStorageProviderProxy.CurrentFile = null;
                    DestStorageProviderProxy.CurrentPath = item.Path;
                    DestStorageProviderProxy.Files.Clear();
                    DestStorageProviderProxy.Files.AddRange(fileItems);
                }
            });
        }

        public void OnDestPathClicked(Control sender, RootItemInfo item)
        {
            foreach (var path in DestStorageProviderProxy.Paths)
            {
                path.Selected = false;
            }
            item.Selected = true;
        }

        public void OnDestFileClicked(object sender, FileItemInfo item)
        {
            DestStorageProviderProxy.CurrentFile = item;
            foreach (var file in DestStorageProviderProxy.Files)
            {
                file.Selected = false;
            }
            item.Selected = true;
        }

        public void OnDestContextMenuOpening(object? sender, CancelEventArgs e)
        {
            var isWritableFrom = !DestStorageProviderProxy.IsReadOnly;
            var isWritableTarget = !SourceStorageProviderProxy.IsReadOnly;
            var isFileOrDir = !DestStorageProviderProxy.CurrentFile?.Name.Equals("..") ?? true;
            DestFileContextPermissions.CanCreateFolder = isWritableFrom;
            DestFileContextPermissions.CanRename = isWritableFrom && isFileOrDir;
            DestFileContextPermissions.CanDelete = isWritableFrom && isFileOrDir;
            DestFileContextPermissions.CanCopyToTarget = isWritableTarget && isFileOrDir;
        }

        public async Task ConnectInternal(StorageProviderProxy storageProviderProxy)
        {
            await storageProviderProxy.ConnectAsync(success =>
            {
                if (success)
                {
                    if (!storageProviderProxy.TryGetRootItems(storageProviderProxy.SelectedConnection, out var rootItems))
                    {
                        storageProviderProxy.Disconnect();
                        return;
                    }

                    if (rootItems.Count() > 0)
                    {
                        var path = string.IsNullOrEmpty(storageProviderProxy.CurrentPath) ? rootItems.First().Path : storageProviderProxy.CurrentPath;
                        if (!storageProviderProxy.TryGetFileItems(storageProviderProxy.SelectedConnection, path, out var fileItems))
                        {
                            storageProviderProxy.Disconnect();
                            return;
                        }

                        Dispatcher.UIThread.Invoke(() => {
                            storageProviderProxy.CurrentPath = path;
                            storageProviderProxy.Paths.Clear();
                            storageProviderProxy.Paths.AddRange(rootItems);
                            storageProviderProxy.Files.Clear();
                            storageProviderProxy.Files.AddRange(fileItems);
                        });
                    }
                }
            });
        }

        public async Task Connect(StorageProviderProxy storageProviderProxy)
        {
            if (Owner == null)
            {
                return;
            }

            if (string.IsNullOrEmpty(storageProviderProxy.CurrentPath) && storageProviderProxy.SelectedConnection is ConnectionFTP connectionFtp)
            {
                var ftpPickerDialogWindow = new FtpPickerDialogWindow();
                var ftpPickerDialogWindowViewModel = new FtpPickerDialogWindowViewModel { Owner = ftpPickerDialogWindow };
                ftpPickerDialogWindowViewModel.OnResult += async ok =>
                {
                    if (!ok || ftpPickerDialogWindowViewModel.SelectedFtpDetail == null)
                    {
                        return;
                    }
                    connectionFtp.FtpDetails = ftpPickerDialogWindowViewModel.SelectedFtpDetail;
                    await ConnectInternal(storageProviderProxy);
                };
                ftpPickerDialogWindow.DataContext = ftpPickerDialogWindowViewModel;
                await ftpPickerDialogWindow.ShowDialog(Owner);
                return;
            }
  
            await ConnectInternal(storageProviderProxy);
        }

        public void Disconnect(StorageProviderProxy storageProviderProxy)
        {
            Dispatcher.UIThread.Invoke(() =>
            {
                storageProviderProxy.Disconnect();
                storageProviderProxy.Paths.Clear();
                storageProviderProxy.Files.Clear();
            });
        }

        public ObservableCollection<LogDetail> LogDetails { get; }

        public ObservableCollection<DownloadDetail> DownloadDetails { get; }

        public MainWindowViewModel()
        {
            LogDetails = new ObservableCollection<LogDetail>(Array.Empty<LogDetail>());
            DownloadDetails = new ObservableCollection<DownloadDetail>(Array.Empty<DownloadDetail>());

            var sourceConnections = new ObservableCollection<IConnection>
            {
                new ConnectionXBINS(),
                new ConnectionLocal(),
                new ConnectionFTP(),
            };

            var destConnections = new ObservableCollection<IConnection>
            {
                new ConnectionLocal(),
                new ConnectionFTP(),
            };

            var logDetailLogger = new LogDetailLogger(LogDetails);
            var connectionManager = new ConnectionManager(logDetailLogger);

            SourceStorageProviderProxy = new StorageProviderProxy(logDetailLogger, sourceConnections, connectionManager);
            DestStorageProviderProxy = new StorageProviderProxy(logDetailLogger, destConnections, connectionManager);

            ExitCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });

            FTPManageCommand = ReactiveCommand.Create(() =>
            {
                if (Owner == null)
                {
                    return;
                }
                var ftpDetailsWindow = new FtpDetailsWindow();
                ftpDetailsWindow.DataContext = new FtpDetailsWindowViewModel { Owner = ftpDetailsWindow };
                ftpDetailsWindow.ShowDialog(Owner);
            });

            SourceConnectCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                if (SourceStorageProviderProxy.Connected)
                {
                    Disconnect(SourceStorageProviderProxy);
                    return;
                }

                await Connect(SourceStorageProviderProxy);
            });

            SourceCreateFolderCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var inputDialogWindow = new InputDialogWindow();
                var inputDialogWindowViewModel = new InputDialogWindowViewModel { Owner = inputDialogWindow, Prompt = "Please enter folder name..." };
                inputDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }

                    if (SourceStorageProviderProxy.CreateFolder(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CombinePath(SourceStorageProviderProxy.CurrentPath, inputDialogWindowViewModel.Input)))
                    {
                        if (!SourceStorageProviderProxy.TryGetFileItems(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(SourceStorageProviderProxy);
                            return;
                        }

                        SourceStorageProviderProxy.Files.Clear();
                        SourceStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                inputDialogWindow.DataContext = inputDialogWindowViewModel;
                await inputDialogWindow.ShowDialog(Owner);
            });

            SourceRenameCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var inputDialogWindow = new InputDialogWindow();
                var inputDialogWindowViewModel = new InputDialogWindowViewModel { Owner = inputDialogWindow, Prompt = "Please enter new name...", Input = SourceStorageProviderProxy.CurrentFile?.Name ?? "" };
                inputDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    if (SourceStorageProviderProxy.CurrentFile != null && SourceStorageProviderProxy.Rename(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentFile, inputDialogWindowViewModel.Input))
                    {
                        if (!SourceStorageProviderProxy.TryGetFileItems(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(SourceStorageProviderProxy);
                            return;
                        }

                        SourceStorageProviderProxy.Files.Clear();
                        SourceStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                inputDialogWindow.DataContext = inputDialogWindowViewModel;
                await inputDialogWindow.ShowDialog(Owner);
            });

            SourceDeleteCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var okDialogWindow = new OkDialogWindow();
                var okDialogWindowViewModel = new OkDialogWindowViewModel { Owner = okDialogWindow, Prompt = "Are you sure you want to delete?" };
                okDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    if (SourceStorageProviderProxy.CurrentFile != null && SourceStorageProviderProxy.Delete(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentFile))
                    {
                        if (!SourceStorageProviderProxy.TryGetFileItems(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(SourceStorageProviderProxy);
                            return;
                        }

                        SourceStorageProviderProxy.Files.Clear();
                        SourceStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                okDialogWindow.DataContext = okDialogWindowViewModel;
                await okDialogWindow.ShowDialog(Owner);
            });

            SourceCopyCommand = ReactiveCommand.Create(() =>
            {
                if (SourceStorageProviderProxy.CurrentFile == null)
                {
                    return;
                }

                var fromName = SourceStorageProviderProxy.CurrentFile.Name;
                var fromPath = SourceStorageProviderProxy.CurrentFile.Path;
                var targetPath = DestStorageProviderProxy.CombinePath(DestStorageProviderProxy.CurrentPath, fromName);
                if (fromPath.Equals(targetPath))
                {
                    // cant copy to self
                    return;
                }

                var fileItems = new List<FileItemInfo>();
                if (SourceStorageProviderProxy.CurrentFile.IsDirectory)
                {
                    if (!SourceStorageProviderProxy.TryRecurseFileItems(SourceStorageProviderProxy.SelectedConnection, SourceStorageProviderProxy.CurrentFile, out var tempFileItems))
                    {
                        Disconnect(SourceStorageProviderProxy);
                        return;
                    }
                    fileItems.AddRange(tempFileItems);
                }
                else
                {
                    fileItems.Add(SourceStorageProviderProxy.CurrentFile);
                }

                foreach (var fileItem in fileItems)
                {
                    var destFolder = fileItem.Path.Substring(fromPath.Length);
                    var downloadDetail = new DownloadDetail
                    {
                        Transferring = false,
                        Progress = "Pending",
                        FileSize = fileItem.Size,
                        SourceConnection = SourceStorageProviderProxy.SelectedConnection,
                        SourcePath = fileItem.Path,
                        DestConnection = DestStorageProviderProxy.SelectedConnection,
                        DestPath = DestStorageProviderProxy.CombinePath(targetPath, destFolder),
                        IsDirectory = fileItem.IsDirectory
                    };
                    DownloadDetails.Add(downloadDetail);
                }
            });

            DestConnectCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                if (DestStorageProviderProxy.Connected)
                {
                    Disconnect(DestStorageProviderProxy);
                    return;
                }
                await Connect(DestStorageProviderProxy);
            });

            DestCreateFolderCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var inputDialogWindow = new InputDialogWindow();
                var inputDialogWindowViewModel = new InputDialogWindowViewModel { Owner = inputDialogWindow, Prompt = "Please enter folder name..." };
                inputDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    if (DestStorageProviderProxy.CreateFolder(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CombinePath(DestStorageProviderProxy.CurrentPath, inputDialogWindowViewModel.Input)))
                    {
                        if (!DestStorageProviderProxy.TryGetFileItems(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(DestStorageProviderProxy);
                            return;
                        }

                        DestStorageProviderProxy.Files.Clear();
                        DestStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                inputDialogWindow.DataContext = inputDialogWindowViewModel;
                await inputDialogWindow.ShowDialog(Owner);
            });

            DestRenameCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var inputDialogWindow = new InputDialogWindow();
                var inputDialogWindowViewModel = new InputDialogWindowViewModel { Owner = inputDialogWindow, Prompt = "Please enter new name...", Input = DestStorageProviderProxy.CurrentFile?.Name ?? "" };
                inputDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    if (DestStorageProviderProxy.CurrentFile != null && DestStorageProviderProxy.Rename(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentFile, inputDialogWindowViewModel.Input))
                    {
                        if (!DestStorageProviderProxy.TryGetFileItems(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(DestStorageProviderProxy);
                            return;
                        }

                        DestStorageProviderProxy.Files.Clear();
                        DestStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                inputDialogWindow.DataContext = inputDialogWindowViewModel;
                await inputDialogWindow.ShowDialog(Owner);
            });

            DestDeleteCommand = ReactiveCommand.Create(async () =>
            {
                if (Owner == null)
                {
                    return;
                }
                var okDialogWindow = new OkDialogWindow();
                var okDialogWindowViewModel = new OkDialogWindowViewModel { Owner = okDialogWindow, Prompt = "Are you sure you want to delete?" };
                okDialogWindowViewModel.OnResult += ok =>
                {
                    if (!ok)
                    {
                        return;
                    }
                    if (DestStorageProviderProxy.CurrentFile != null && DestStorageProviderProxy.Delete(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentFile))
                    {
                        if (!DestStorageProviderProxy.TryGetFileItems(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentPath, out var fileItems))
                        {
                            Disconnect(DestStorageProviderProxy);
                            return;
                        }

                        DestStorageProviderProxy.Files.Clear();
                        DestStorageProviderProxy.Files.AddRange(fileItems);
                        return;
                    }
                    // show failed
                };
                okDialogWindow.DataContext = okDialogWindowViewModel;
                await okDialogWindow.ShowDialog(Owner);
            });

            DestCopyCommand = ReactiveCommand.Create(() =>
            {
                if (DestStorageProviderProxy.CurrentFile == null)
                {
                    return;
                }

                var fromName = DestStorageProviderProxy.CurrentFile.Name;
                var fromPath = DestStorageProviderProxy.CurrentFile.Path;
                var targetPath = SourceStorageProviderProxy.CombinePath(SourceStorageProviderProxy.CurrentPath, fromName);
                if (fromPath.Equals(targetPath))
                {
                    // cant copy to self
                    return;
                }

                var fileItems = new List<FileItemInfo>();
                if (DestStorageProviderProxy.CurrentFile.IsDirectory)
                {
                    if (!DestStorageProviderProxy.TryRecurseFileItems(DestStorageProviderProxy.SelectedConnection, DestStorageProviderProxy.CurrentFile, out var tempFileItems))
                    {
                        Disconnect(DestStorageProviderProxy);
                        return;
                    }
                    fileItems.AddRange(tempFileItems);
                }
                else
                {
                    fileItems.Add(DestStorageProviderProxy.CurrentFile);
                }

                foreach (var fileItem in fileItems)
                {
                    var destFolder = fileItem.Path.Substring(fromPath.Length);
                    var downloadDetail = new DownloadDetail
                    {
                        Transferring = false,
                        Completed = false,
                        Progress = "Pending",
                        FileSize = fileItem.Size,
                        SourceConnection = DestStorageProviderProxy.SelectedConnection,
                        SourcePath = fileItem.Path,
                        DestConnection = SourceStorageProviderProxy.SelectedConnection,
                        DestPath = SourceStorageProviderProxy.CombinePath(targetPath, destFolder),
                        IsDirectory = fileItem.IsDirectory
                    };
                    DownloadDetails.Add(downloadDetail);
                }
            });

            ClearQueueCommand = ReactiveCommand.Create(() =>
            {
                foreach (var item in DownloadDetails)
                {
                    item.CancellationTokenSource.Cancel();
                }
                DownloadDetails.Clear();
            });

            ClearCompletedCommand = ReactiveCommand.Create(() =>
            {
                for (var i = DownloadDetails.Count - 1; i >= 0; i--)
                {
                    if (DownloadDetails[i].Completed)
                    {
                        DownloadDetails.RemoveAt(i);
                    }
                }
            });

            RetryFailedCommand = ReactiveCommand.Create(() =>
            {
                foreach (var item in DownloadDetails)
                {
                    if (!item.Failed)
                    {
                        continue;
                    }
                    item.Progress = "Pending";
                    item.Transferring = false;
                    item.Completed = false;
                    item.Failed = false;
                }
            });



            DownloadQueueProcessor = new DownloadQueueProcessor(logDetailLogger, DownloadDetails, connectionManager);
            DownloadQueueProcessor.Start();
            DownloadQueueProcessor.Paused = true;

            UnpauseCommand = ReactiveCommand.Create(() =>
            {
                DownloadQueueProcessor.Paused = false;
            });
        }
    }
}
