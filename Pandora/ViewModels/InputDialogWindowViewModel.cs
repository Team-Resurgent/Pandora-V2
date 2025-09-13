using Avalonia.Controls;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace Pandora.ViewModels
{
    public partial class InputDialogWindowViewModel : ViewModelBase
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

        private string _input;
        public string Input
        {
            get => _input;
            set => this.RaiseAndSetIfChanged(ref _input, value);
        }

        public event Action<bool>? OnResult;

        public InputDialogWindowViewModel()
        {
            _prompt = string.Empty;
            _input = string.Empty;

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
