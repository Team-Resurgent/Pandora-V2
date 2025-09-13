namespace Pandora.Models
{
    public class FtpDetails
    {
        public string Name { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string User { get; set; }

        public string Password { get; set; }

        public FtpDetails() 
        {
            Name = string.Empty;
            Host = string.Empty;
            Port = 21;
            User = string.Empty;
            Password = string.Empty;
        }
    }
}
