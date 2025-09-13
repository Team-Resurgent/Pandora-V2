using Avalonia.Threading;
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

        private readonly ConcurrentQueue<DownloadDetail> _queue = new();
        private readonly ObservableCollection<DownloadDetail> _observableCollection;
        private readonly ConcurrentDictionary<DownloadDetail, bool> _canceledItems = new();
        private readonly CancellationTokenSource _cts = new();
        private readonly StorageProviderProxy _sourceStorageProviderProxy;
        private readonly StorageProviderProxy _destStorageProviderProxy;

        public DownloadQueueProcessor(StorageProviderProxy sourceStorageProviderProxy, StorageProviderProxy destStorageProviderProxy, ObservableCollection<DownloadDetail> observableCollection)
        {
            _observableCollection = observableCollection;
            _observableCollection.CollectionChanged += OnCollectionChanged;
            _sourceStorageProviderProxy = sourceStorageProviderProxy;
            _destStorageProviderProxy = destStorageProviderProxy;
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
                if (_queue.TryDequeue(out var item))
                {
                    if (_canceledItems.TryRemove(item, out _))
                    {
                        continue;
                    }

                    if (item.IsDirectory)
                    {
                        // fixme
                        _destStorageProviderProxy.CreateFolder(item.DestPath);
                    }
                    else
                    {
                        using var fromStream = _sourceStorageProviderProxy.OpenReadStream(item.SourcePath, item.FileSize);
                        using var targetStream = _destStorageProviderProxy.OpenWriteStream(item.DestPath, item.FileSize);
                        if (fromStream != null && targetStream != null)
                        {
                            FileSystemHelper.CopyStreamWithProgress(item.CancellationTokenSource.Token, fromStream, targetStream, 0, (p) =>
                            {
                                Dispatcher.UIThread.Invoke(() =>
                                {
                                    item.Progress = $"{p:P}";
                                });
                            });
                            Dispatcher.UIThread.Invoke(() => {
                                item.Transferring = false;
                                item.Completed = true;
                                item.Progress = "Done";
                            });
                        }
                        else
                        {
                            Dispatcher.UIThread.Invoke(() =>
                            {
                                item.Transferring = false;
                                item.Failed = true;
                                item.Progress = $"Failed";
                            });
                        }
                    }
                }
                else
                {
                    await Task.Delay(100); // nothing to process
                }
            }
        }
    }
}
