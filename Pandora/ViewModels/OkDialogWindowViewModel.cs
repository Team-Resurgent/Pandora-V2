using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class OkDialogWindowViewModel : ViewModelBase
    {
        public Window? Owner { get; set; }

        public ICommand OkCommand { get; }

        public ICommand CancelCommand { get; }

        private string _prompt;
        public string Prompt
        {
            get => _prompt;
            set => this.RaiseAndSetIfChanged(ref _prompt, value);
        }

        public event Action<bool>? OnResult;

        public OkDialogWindowViewModel()
        {
            _prompt = string.Empty;

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
        }
    }
}
