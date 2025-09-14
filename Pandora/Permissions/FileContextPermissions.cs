using ReactiveUI;

namespace Pandora.Permissions
{
    public class FileContextPermissions : ReactiveObject
    {
        private bool _canCreateFolder;
        public bool CanCreateFolder
        {
            get => _canCreateFolder;
            set => this.RaiseAndSetIfChanged(ref _canCreateFolder, value);
        }

        private bool _canRename;
        public bool CanRename
        {
            get => _canRename;
            set => this.RaiseAndSetIfChanged(ref _canRename, value);
        }

        private bool _canDelete;
        public bool CanDelete
        {
            get => _canDelete;
            set => this.RaiseAndSetIfChanged(ref _canDelete, value);
        }

        private bool _canCopyToTarget;
        public bool CanCopyToTarget
        {
            get => _canCopyToTarget;
            set => this.RaiseAndSetIfChanged(ref _canCopyToTarget, value);
        }

        private bool _canRefresh;
        public bool CanRefresh
        {
            get => _canRefresh;
            set => this.RaiseAndSetIfChanged(ref _canRefresh, value);
        }

        public void ClearPermissions()
        {
            CanCreateFolder = false;
            CanRename = false;
            CanDelete = false;
            CanCopyToTarget = false;
            CanRefresh = false;
        }
    }
}
