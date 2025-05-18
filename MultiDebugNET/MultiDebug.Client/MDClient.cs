using System.Net.Sockets;
using System.Text;

namespace MultiDebug.Client
{
    public class MDClient
    {
        private static TcpClient _client = null!;
        private static NetworkStream _stream = null!;
        private static StreamReader _reader = null!;
        private static StreamWriter _writer = null!;
        private static CancellationTokenSource _cts = null!;
        private static bool _authenticated = false;

        public static async void Connect(ILogger logger, string Host, int Port)
        {
            _client = new TcpClient();
            _cts = new CancellationTokenSource();
            try
            {
                await _client.ConnectAsync(Host, Port);
                logger.Log(MsgType.None, $"[CLIENT] Connected to {Host}:{Port}!");
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream, Encoding.UTF8);
                _writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true };

                logger.onConnect();

                _ = ListenAsync(logger, _cts.Token);
            }
            catch (Exception ex)
            {
                logger.Log(MsgType.None, $"[CLIENT] Connect error: {ex.Message}");
            }
        }

        public static void Disconnect(ILogger logger)
        {
            try
            {
                _cts?.Cancel();

                _writer?.Close();
                _reader?.Close();
                _stream?.Close();
                _client?.Close();

                _authenticated = false;

                logger.onDisconnect();
                logger.Log(MsgType.None, $"[CLIENT] Disconnected!");
            }
            catch (Exception ex)
            {
                logger.Log(MsgType.None, $"[CLIENT] Disconnect error: {ex.Message}");
            }
        }

        public static async void Send(ILogger logger, string Content)
        {
            if (string.IsNullOrWhiteSpace(Content)) return;

            logger.Log(MsgType.None, $"> {Content}");

            switch (Content.ToLower())
            {
                case "exit":
                case "disconnect":
                    logger.ClearInputLine();
                    Disconnect(logger);
                    return;

                case "cls":
                case "clear":
                    logger.ClearInputLine();
                    logger.LogClear();
                    return;

                default:
                    break;
            }

            try
            {
                await _writer.WriteLineAsync(Content);
                logger.ClearInputLine();
            }
            catch (Exception ex)
            {
                logger.Log(MsgType.None, $"[CLIENT] Send error: {ex.Message}");
            }
        }

        private static async Task ListenAsync(ILogger logger, CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    var line = await _reader.ReadLineAsync();
                    if (line == null) break;

                    if (!SrvMessage.TryParse(line, out var message))
                    {
                        logger.Log(MsgType.None, "[UNKNOWN] " + line);
                        continue;
                    }

                    logger.Log(message.Type, message.Type switch
                    {
                        MsgType.Info => "[INFO] " + message.Content,
                        MsgType.Debug => "[DEBUG] " + message.Content,
                        MsgType.Warning => "[WARNING] " + message.Content,
                        MsgType.Error => "[ERROR] " + message.Content,
                        MsgType.Passwd => "[SERVER] Password: ",
                        _ => "" + line
                    });

                    if (message.Type == MsgType.Error && message.Content == "Invalid password") Disconnect(logger);

                    if (message.Type == MsgType.Info && message.Content == "Authenticated")
                        _authenticated = true;
                }
            }
            catch (OperationCanceledException)
            {

            }
            catch (Exception ex)
            {
                logger.Log(MsgType.None, $"[CLIENT] Listen error: {ex.Message}");
                Disconnect(logger);
            }
        }
    }
}
