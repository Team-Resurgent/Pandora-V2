using Pandora.Models;
using System.IO;
using System.Text.Json;

namespace Pandora.Utils
{
    public class Config
    {
        public string[] EffnetServers { get; set; }

        public FtpDetails[] FtpServers { get; set; }

        public Config() 
        {
            EffnetServers = [
                "irc.servercentral.net",
                "irc.prison.net",
                "irc.underworld.no",
                "efnet.port80.se",
                "efnet.deic.eu",
                "irc.efnet.nl",
                "irc.swepipe.se",
                "irc.efnet.fr",
                "irc.choopa.net",
            ];
            FtpServers = [];
        }

        public static Config LoadConfig()
        {
            var applicationPath = FileSystemHelper.GetApplicationPath();
            if (applicationPath == null)
            {
                return new Config();
            }
            var configPath = Path.Combine(applicationPath, "config.json");
            if (File.Exists(configPath))
            {
                var configJson = File.ReadAllText(configPath);
                var result = JsonSerializer.Deserialize<Config>(configJson);
                return result ?? new Config();
            }
            var config = new Config();
            SaveConfig(config);
            return config;
        }

        public static void SaveConfig(Config config)
        {
            var applicationPath = FileSystemHelper.GetApplicationPath();
            if (applicationPath == null)
            {
                return;
            }
            var configPath = Path.Combine(applicationPath, "config.json");
            var result = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, result);
        }
    }
}
