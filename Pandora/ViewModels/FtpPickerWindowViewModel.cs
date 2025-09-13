using Avalonia.Controls;
using Pandora.Models;
using Pandora.Utils;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class FtpPickerWindowViewModel : ViewModelBase
    {
        public Window? Owner { get; set; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        public ObservableCollection<FtpDetails> FtpDetailsList { get; }

        private FtpDetails? _selectedFtpDetail;
        public FtpDetails? SelectedFtpDetail
        {
            get => _selectedFtpDetail;
            set => this.RaiseAndSetIfChanged(ref _selectedFtpDetail, value);
        }

        public FtpPickerWindowViewModel()
        {
            OkCommand = ReactiveCommand.Create(() =>
            {

            });

            CancelCommand = ReactiveCommand.Create(() =>
            {
                //if (Owner is FtpDetailsWindow ftpDetailsWindow)
                //{
                //    var index = ftpDetailsWindow.FtpDetailsGrid.SelectedIndex;
                //    if (index >= 0)
                //    {
                //        FtpDetailsList?.RemoveAt(index);
                //    }
                //}
            });


            var config = Config.LoadConfig();
            FtpDetailsList = [.. config.FtpServers];
        }
    }
}
