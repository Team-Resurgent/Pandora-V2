using Avalonia;
using Avalonia.ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Pandora
{
    internal sealed class Program
    {
        private static void LogException(string type, Exception? ex)
        {
            try
            {
                var logPath = Path.Combine(AppContext.BaseDirectory, "crashlog.txt");
                File.AppendAllText(logPath, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {type}: {ex?.ToString() ?? "Unknown error"}{Environment.NewLine}");
            }
            catch
            {
                // Ignore logging failures
            }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                LogException("UnhandledException", e.ExceptionObject as Exception);
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                LogException("UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            try
            {
                BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
            }
            catch (Exception ex)
            {
                LogException("FatalException", ex);
                throw; 
            }
        }

        // Avalonia configuration, don't remove; also used by visual designer.
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .WithInterFont()
                .LogToTrace()
                .UseReactiveUI();
    }
}
