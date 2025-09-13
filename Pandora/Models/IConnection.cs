namespace Pandora.Models
{
    public enum ConnectionType
    {
        Local,
        XBINS,
        FTP
    }

    public enum Protocol
    {
        Undefined,
        Local,
        FTP
    }

    public interface IConnection
    {
        ConnectionType ConnectionType { get; }

        string Name { get; }


    }

    public class ConnectionLocal : IConnection
    {
        public ConnectionType ConnectionType => ConnectionType.Local;

        public string Name => "Local";
    }

    public class ConnectionXBINS : IConnection
    {
        public ConnectionType ConnectionType => ConnectionType.XBINS;

        public string Name => "XBINS";
    }

    public class ConnectionFTP : IConnection
    {
        public ConnectionType ConnectionType => ConnectionType.FTP;

        public FtpDetails? FtpDetails { get; set; }

        public string Name => FtpDetails == null ? "FTP" : $"FTP - {FtpDetails?.Name ?? string.Empty}";
    }
}
