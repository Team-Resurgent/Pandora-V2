using ReactiveUI;

namespace Pandora.Models
{
    public class FileItemInfo : ReactiveObject
    {
        private bool _isDirectory;
        public bool IsDirectory
        {
            get => _isDirectory;
            set => this.RaiseAndSetIfChanged(ref _isDirectory, value);
        }

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        private string _path = string.Empty;
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        private long _size;
        public long Size
        {
            get => _size;
            set => this.RaiseAndSetIfChanged(ref _size, value);
        }

        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set => this.RaiseAndSetIfChanged(ref _selected, value);
        }
    }
}