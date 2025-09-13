using Avalonia.Controls;
using Pandora.ViewModels;

namespace Pandora;

public partial class FtpDetailsWindow : Window
{


    private void FtpDetailsListCellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
    {
        if (DataContext is FtpDetailsWindowViewModel vm)
        {
            vm.FtpDetailsListCellEditEnding(sender, e);
        }
    }

    public FtpDetailsWindow()
    {
        InitializeComponent();
    }
}