using ReactiveUI;
using System.Threading;

namespace Pandora.Models
{
    public class DownloadDetail : ReactiveObject
    {
        private bool _transferring;
        public bool Transferring
        {
            get => _transferring;
            set => this.RaiseAndSetIfChanged(ref _transferring, value);
        }

        private bool _completed;
        public bool Completed
        {
            get => _completed;
            set => this.RaiseAndSetIfChanged(ref _completed, value);
        }

        private bool _failed;
        public bool Failed
        {
            get => _failed;
            set => this.RaiseAndSetIfChanged(ref _failed, value);
        }

        private string _progress = string.Empty;
        public string Progress
        {
            get => _progress;
            set => this.RaiseAndSetIfChanged(ref _progress, value);
        }

        private string _sourcePath = string.Empty;
        public string SourcePath
        {
            get => _sourcePath;
            set => this.RaiseAndSetIfChanged(ref _sourcePath, value);
        }

        private string _destPath = string.Empty;
        public string DestPath
        {
            get => _destPath;
            set => this.RaiseAndSetIfChanged(ref _destPath, value);
        }

        private bool _isDirectory;
        public bool IsDirectory
        {
            get => _isDirectory;
            set => this.RaiseAndSetIfChanged(ref _isDirectory, value);
        }

        private long _fileSize;
        public long FileSize
        {
            get => _fileSize;
            set => this.RaiseAndSetIfChanged(ref _fileSize, value);
        }

        private IConnection? _sourceConnection;
        public IConnection? SourceConnection
        {
            get => _sourceConnection;
            set => this.RaiseAndSetIfChanged(ref _sourceConnection, value);
        }

        private IConnection? _destConnection;
        public IConnection? DestConnection
        {
            get => _destConnection;
            set => this.RaiseAndSetIfChanged(ref _destConnection, value);
        }

        public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    }
}
