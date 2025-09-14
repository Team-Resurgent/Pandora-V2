using Pandora.Logging;
using Pandora.Models;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Pandora.Utils
{
    public static class XbinsHelper
    {
        private static string GenerateWord(int length)
        {
            string consonants = "bcdfghjklmnpqrstvwxyz";
            string vowels = "aeiou";

            string word = string.Empty;

            var random = new Random(Guid.NewGuid().GetHashCode());

            if (random.Next() % 2 == 0)
            {
                word += consonants[random.Next(0, consonants.Length)];
            }
            else
            {
                word += vowels[random.Next(0, vowels.Length)];
            }

            for (int i = 1; i < length; i += 2)
            {
                char c = consonants[random.Next(0, consonants.Length)];
                char v = vowels[random.Next(0, vowels.Length)];
                if (c == 'q')
                {
                    word += "qu";
                }
                else
                {
                    word += $"{c}{v}";
                }
            }

            if (word.Length < length)
            {
                word += consonants[random.Next(0, consonants.Length)];
            }

            return word;
        }

        private static void SendMessage(ILogger logger, TcpClient tcpClient, CancellationTokenSource? cancellationToken, string message)
        {
            try
            {
                var stream = tcpClient.GetStream();
                var buffer = Encoding.UTF8.GetBytes($"{message}\r\n");
                stream.Write(buffer, 0, buffer.Length);
                logger.LogMessage("Sent", message);
            }
            catch (Exception ex)
            {
                logger.LogMessage("Error", $"SendMessage: Exception occured, disconnecting. '{ex.Message}'.");
                cancellationToken?.Cancel();
            }
        }

        private static string ReadResponse(ILogger logger, TcpClient tcpClient, CancellationTokenSource? cancellationToken)
        {
            try
            {
                var stream = tcpClient.GetStream();
                var response = new StringBuilder();
                while (stream.DataAvailable)
                {
                    var value = (char)stream.ReadByte();
                    if (value == '\r')
                    {
                        continue;
                    }
                    if (value == '\n')
                    {
                        break;
                    }
                    response.Append(value);
                }
                var message = response.ToString();
                logger.LogMessage("Recieved", message);
                return message;
            }
            catch (Exception ex)
            {
                logger.LogMessage("Error", $"ReadResponse: Exception occured, disconnecting. '{ex.Message}'.");
                cancellationToken?.Cancel();
                return string.Empty;
            }
        }

        private static FtpDetails? ProcessPrivMsg(string argument)
        {
            if (!argument.Contains("FTP ADDRESS"))
            {
                return null;
            }
            var parts = argument.Split('', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 10)
            {
                return null;
            }
            var host = parts[1];
            if (!int.TryParse(parts[3], out var port))
            {
                return null;
            }
            var user = parts[5];
            var pass = parts[7];
            return new FtpDetails { Name = "XBINS", Host = host, Port = port, User = user, Password = pass };
        }

        private static FtpDetails? ProcessResponse(ILogger logger, TcpClient tcpClient, CancellationTokenSource? cancellationToken, string response)
        {
            if (response.Length == 0)
            {
                return null;
            }

            var prefix = string.Empty;

            var startOffset = 0;
            if (response[0] == ':')
            {
                startOffset = 1;
                while (startOffset < response.Length)
                {
                    char value = response[startOffset];
                    if (value == ' ')
                    {
                        startOffset++;
                        break;
                    }
                    prefix += value;
                    startOffset++;
                }
            }

            var position = response.IndexOf(':', startOffset);
            if (position < 0)
            {
                return null;
            }

            var commandParts = response.Substring(startOffset, position - startOffset).Trim().Split(' ');
            var argument = response.Substring(position + 1).Trim();
            switch (commandParts[0])
            {
                case "001":
                    SendMessage(logger, tcpClient, cancellationToken, $"JOIN #xbins");
                    break;
                case "332":
                    SendMessage(logger, tcpClient, cancellationToken, $"PRIVMSG #xbins :!list");
                    break;
                case "PRIVMSG":
                    return ProcessPrivMsg(argument);
                case "ERROR":
                    cancellationToken?.Cancel();
                    break;
                case "PING":
                    SendMessage(logger, tcpClient, cancellationToken, $"PONG :{argument}");
                    break;
            }

            return null;
        }

        private static bool TryGetFtpDetails(ILogger logger, TcpClient tcpClient, CancellationTokenSource? cancellationToken, out FtpDetails ftpDetails)
        {
            SendMessage(logger, tcpClient, cancellationToken, $"NICK {GenerateWord(10)}");
            SendMessage(logger, tcpClient, cancellationToken, $"USER {GenerateWord(10)} . . {GenerateWord(10)}");

            ftpDetails = new FtpDetails();

            try
            {
                var stream = tcpClient.GetStream();
                while (!cancellationToken?.IsCancellationRequested ?? false)
                {
                    if (stream.DataAvailable)
                    {
                        var resonse = ReadResponse(logger, tcpClient, cancellationToken);
                        var tempFtpDetails = ProcessResponse(logger, tcpClient, cancellationToken, resonse);
                        if (tempFtpDetails != null)
                        {
                            ftpDetails = tempFtpDetails;
                            return true;
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                SendMessage(logger, tcpClient, cancellationToken, "QUIT");
            }
            catch (Exception ex)
            {
                logger.LogMessage("Error", $"Disconnecting as exception occured '{ex.Message}'.");
            }
            return false;
        }

        public static FtpDetails? ConnectIrc(ILogger logger, CancellationTokenSource cancellationToken)
        {
            var config = Config.LoadConfig();

            var servers = config.EffnetServers;
            for (var i = 0; i < servers.Length; i++)
            {
                if (cancellationToken?.IsCancellationRequested ?? false)
                {
                    break;
                }
                logger.LogMessage("Connecting...", $"Tring to connect to '{servers[i]}'.");
                var tcpClient = new TcpClient();
                var result = tcpClient.BeginConnect(servers[i], 6667, null, null);
                var waitHandle = result.AsyncWaitHandle;
                try
                {
                    if (!result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(30), false))
                    {
                        tcpClient.Close();
                        tcpClient.Dispose();
                    }
                    else
                    {
                        tcpClient.EndConnect(result);

                        if (TryGetFtpDetails(logger, tcpClient, cancellationToken, out var ftpDetails))
                        {
                            return ftpDetails;
                        }

                        tcpClient.Close();
                        tcpClient.Dispose();

                    }
                }
                catch
                {
                    // do nothing
                }
                finally
                {
                    waitHandle.Close();
                }
            }

            return null;
        }
    }
}
