using Avalonia.Threading;
using Pandora.Models;
using System.Collections.ObjectModel;

namespace Pandora.Logging
{
    public class LogDetailLogger : ILogger
    {
        private ObservableCollection<LogDetail> _logDetails;

        public LogDetailLogger(ObservableCollection<LogDetail> logDetails)
        {
            _logDetails = logDetails;
        }

        public void LogMessage(string type, string message)
        {
            Dispatcher.UIThread.Post(() =>
            {
                _logDetails.Add(new LogDetail(type, message));
            });
        }
    }
}
