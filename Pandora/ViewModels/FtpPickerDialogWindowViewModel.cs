using Avalonia.Controls;
using Pandora.Models;
using Pandora.Utils;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class FtpPickerDialogWindowViewModel : ViewModelBase
    {
        public Window? Owner { get; set; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public ObservableCollection<FtpDetails> FtpDetailsList { get; }

        public bool OkEnabled => SelectedFtpDetail != null;

        public FtpDetails? _selectedFtpDetail;
        public FtpDetails? SelectedFtpDetail
        {
            get => _selectedFtpDetail;
            set {
                this.RaiseAndSetIfChanged(ref _selectedFtpDetail, value);
                this.RaisePropertyChanged(nameof(OkEnabled));
            }
        }

        public event Action<bool>? OnResult;

        public FtpPickerDialogWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {
                OnResult?.Invoke(true);
                Owner?.Close();
            });

            CancelCommand = ReactiveCommand.Create(() =>
            {
                OnResult?.Invoke(false);
                Owner?.Close();
            });

            var config = Config.LoadConfig();
            FtpDetailsList = [.. config.FtpServers];
        }
    }
}
