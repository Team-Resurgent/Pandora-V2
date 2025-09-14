using Avalonia.Threading;
using Pandora.Logging;
using Pandora.Models;
using Pandora.Storage;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Utils
{
    public class DownloadQueueProcessor
    {
        private readonly ILogger _logger;
        private readonly ConcurrentQueue<DownloadDetail> _queue = new();
        private readonly ObservableCollection<DownloadDetail> _observableCollection;
        private readonly ConcurrentDictionary<DownloadDetail, bool> _canceledItems = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly ConnectionManager _connectionManager;

        public bool Paused { get; set; }

        public DownloadQueueProcessor(ILogger logger, ObservableCollection<DownloadDetail> observableCollection, ConnectionManager connectionManager)
        {
            _logger = logger;
            _observableCollection = observableCollection;
            _observableCollection.CollectionChanged += OnCollectionChanged;
            _connectionManager = connectionManager;
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    var downloadDetail = (DownloadDetail)item;
                    System.Diagnostics.Debug.Print($"Enquing: {downloadDetail.SourcePath}");
                    _queue.Enqueue(downloadDetail);
                }
            }

            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    var downloadDetail = (DownloadDetail)item;
                    System.Diagnostics.Debug.Print($"Cancelling: {downloadDetail.SourcePath}");
                    downloadDetail.CancellationTokenSource.Cancel();
                    _canceledItems[downloadDetail] = true;
                }
            }
        }

        public void Start()
        {
            Task.Run(() => ProcessQueueAsync(_cts.Token));
        }

        public void Stop()
        {
            _cts.Cancel();
        }

        private async Task ProcessQueueAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                if (!Paused && _queue.TryDequeue(out var item))
                {
                    if (_canceledItems.TryRemove(item, out _))
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            item.Completed = true;
                            item.Progress = "Cancelled";
                        });
                        continue;
                    }

                    if (item.DestConnection == null || item.SourceConnection == null)
                    {
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            item.Completed = true;
                            item.Progress = "Invalid";
                        });
                        continue;
                    }

                    if (item.IsDirectory)
                    {
                        _connectionManager.CreateFolder(item.DestConnection, item.DestPath);
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            item.Completed = true;
                            item.Progress = "Done";
                        });
                    }
                    else
                    {
                        using var fromStream = _connectionManager.OpenReadStream(item.SourceConnection, item.SourcePath, item.FileSize);
                        using var targetStream = _connectionManager.OpenWriteStream(item.DestConnection, item.DestPath, item.FileSize);

                        if (fromStream == null || targetStream == null)
                        {
                            if (fromStream == null && item.SourceConnection != null)
                            {
                                _connectionManager.Connect(item.SourceConnection);
                            }
                            if (targetStream == null && item.DestConnection != null)
                            {
                                _connectionManager.Connect(item.DestConnection);
                            }
                            _queue.Enqueue(item);
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                item.Progress = $"Reconnecting";
                            });
                            continue;
                        }

                        if (fromStream != null && targetStream != null)
                        {
                            if (FileSystemHelper.CopyStreamWithProgress(item.CancellationTokenSource.Token, fromStream, targetStream, 0, (p) =>
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    item.Progress = $"{p:P}";
                                });
                            }))
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    item.Completed = true;
                                    item.Progress = "Done";
                                });
                                continue;
                            }

                            if (item.CancellationTokenSource.Token.IsCancellationRequested)
                            {
                                continue;
                            }
                        }
        
                        _queue.Enqueue(item);
                        Dispatcher.UIThread.Invoke(() =>
                        {
                            item.Failed = true;
                            item.Progress = $"Failed";
                        });
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
        }
    }
}
