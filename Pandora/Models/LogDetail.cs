namespace Pandora.Models
{
    public class LogDetail
    {
        public string LogType { get; set; }
        public string Message { get; set; }

        public LogDetail(string logType, string message)
        {
            LogType = logType;
            Message = message;
        }
    }
}
