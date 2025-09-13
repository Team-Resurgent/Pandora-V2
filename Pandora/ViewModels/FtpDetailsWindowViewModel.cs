using Avalonia.Controls;
using Pandora.Models;
using Pandora.Utils;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class FtpDetailsWindowViewModel : ViewModelBase
    {
        public Window? Owner { get; set; }

        public ICommand AddCommand { get; }

        public ICommand DeleteCommand { get; }

        public ICommand ExitCommand { get; }

        public ObservableCollection<FtpDetails> FtpDetailsList { get; }

        public void SaveFtpDetails()
        {
            var ftpDetailsList = new List<FtpDetails>();
            var config = Config.LoadConfig();
            if (FtpDetailsList != null)
            {
                for (var i = 0; i < FtpDetailsList.Count; i++)
                {
                    ftpDetailsList.Add(FtpDetailsList[i]);
                }
            }
            config.FtpServers = [.. ftpDetailsList];
            Config.SaveConfig(config);
        }

        public void FtpDetailsListCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            SaveFtpDetails();
        }

        public FtpDetailsWindowViewModel()
        {
            AddCommand = ReactiveCommand.Create(() =>
            {
                FtpDetailsList?.Add(new FtpDetails());
                SaveFtpDetails();
            });

            DeleteCommand = ReactiveCommand.Create(() =>
            {
                if (Owner is FtpDetailsWindow ftpDetailsWindow)
                {
                    var index = ftpDetailsWindow.FtpDetailsGrid.SelectedIndex;
                    if (index >= 0)
                    {
                        FtpDetailsList?.RemoveAt(index);
                    }
                    SaveFtpDetails();
                }
            });

            ExitCommand = ReactiveCommand.Create(() =>
            {
                Owner?.Close();
            });

            var config = Config.LoadConfig();
            FtpDetailsList = [.. config.FtpServers];
        }
    }
}
