using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Pandora.Models;
using Pandora.Utils;
using Pandora.ViewModels;
using ReactiveUI;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace Pandora.Views
{
    public partial class MainWindow : Window
    {
        private void SourcePath_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is RootItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnSourcePathClicked(border, item);
            }
        }

        private void SourceFile_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is FileItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnSourceFileClicked(border, item);
            }
        }

        private void DestPath_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is RootItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnDestPathClicked(border, item);
            }
        }

        private void DestFile_PointerPressed(object sender, PointerPressedEventArgs e)
        {
            if (sender is Border border && border.DataContext is FileItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnDestFileClicked(border, item);
            }
        }




        private void SourcePath_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is RootItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnSourcePathDoubleClicked(border, item);
            }
        }

        private void SourceFile_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is FileItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnSourceFileDoubleClicked(border, item);
            }
        }

        private void DestPath_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is RootItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnDestPathDoubleClicked(border, item);
            }
        }

        private void DestFile_DoubleTapped(object sender, TappedEventArgs e)
        {
            if (sender is Border border && border.DataContext is FileItemInfo item)
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnDestFileDoubleClicked(border, item);
            }
        }

        private void OnLogDetailsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is System.Collections.IList list && list.Count > 0)
            {
                var lastItem = list[list.Count - 1];
                // Ensure scrolling happens on UI thread
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    if (LogDetailGrid.Columns.Count > 0)
                        LogDetailGrid.ScrollIntoView(lastItem, LogDetailGrid.Columns[0]);
                }, Avalonia.Threading.DispatcherPriority.Background);
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            Title = $"Pandora V{Version.Value}";

            Opened += async (_, _) =>
            {
                await Task.Delay(2000);

                var animationFadeIn = Resources["FadeInAnimation"] as Animation;
                var animationFadeOut = Resources["FadeOutAnimation"] as Animation;
                if (animationFadeIn != null && animationFadeOut != null)
                {
                    await Task.WhenAll(
                        animationFadeIn.RunAsync(MainContent),
                        animationFadeOut.RunAsync(SplashView)
                   );
                }
                SplashView.IsVisible = false;
            };

            Closed += (_, _) =>
            {
                SoundPlayer.Cleanup();
            };

            SourceContextMenu.Opening += (sender, e) =>
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnSourceContextMenuOpening(sender, e);
            };

            DestContextMenu.Opening += (sender, e) =>
            {
                var vm = DataContext as MainWindowViewModel;
                vm?.OnDestContextMenuOpening(sender, e);
            };

            // Attach after DataContext is set
            this.DataContextChanged += (sender, _) =>
            {
                if (sender is Window window && window.DataContext is MainWindowViewModel vm)
                {
                    vm.LogDetails.CollectionChanged += OnLogDetailsChanged;
                }
            };
        }
    }
}