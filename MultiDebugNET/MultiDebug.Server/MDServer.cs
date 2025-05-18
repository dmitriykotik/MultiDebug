using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace MultiDebug.Server
{
    public class MDServer
    {
        private readonly TcpListener _listener;
        private readonly bool _isDebug;
        private readonly bool _isEnable;
        private readonly string? _passwordHash;
        private readonly Dictionary<string, Func<ClientContext, string[], Task>> _commands = new();
        private readonly List<ClientContext> _clients = new();

        public MDServer(int port = 5000, bool isDebug = false, bool isEnable = true, string password = "admin")
        {
            _isDebug = isDebug;
            _isEnable = isEnable;
            if (!_isDebug && password != null)
                _passwordHash = ComputeSha256(password);
            _listener = new TcpListener(IPAddress.Any, port);
        }

        public async Task StartAsync(int port = 5000)
        {
            if (!_isEnable) return;
            if (GetLocalIP() == null) return;

            _listener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listener.Start();

            while (true)
            {
                var client = await _listener.AcceptTcpClientAsync();
                _ = HandleClientAsync(client);
            }
        }

        public void RegisterCommand(string name, Func<ClientContext, string[], Task> handler)
        {
            _commands[name] = handler;
        }

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            var client = new ClientContext(tcpClient);

            lock (_clients) _clients.Add(client);

            try
            {
                var writer = new StreamWriter(client.Stream, new UTF8Encoding(false)) { AutoFlush = true };
                var reader = new StreamReader(client.Stream, new UTF8Encoding(false));

                if (!_isDebug)
                {
                    await writer.WriteLineAsync(new SrvMessage(MsgType.Passwd, "Enter password:").ToString());
                    var input = await reader.ReadLineAsync();
                    var hash = ComputeSha256(input ?? "");
                    if (hash != _passwordHash)
                    {
                        await writer.WriteLineAsync(new SrvMessage(MsgType.Error, "Invalid password").ToString());
                        tcpClient.Close();
                        return;
                    }
                }

                client.IsAuthenticated = true;
                await writer.WriteLineAsync(new SrvMessage(MsgType.Info, "Authenticated").ToString());

                while (true)
                {
                    var line = await reader.ReadLineAsync();
                    if (line == null) break;

                    var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 0) continue;

                    var cmd = parts[0];
                    var args = parts.Skip(1).ToArray();

                    if (_commands.TryGetValue(cmd, out var handler))
                        await handler(client, args);
                    else
                        await writer.WriteLineAsync(new SrvMessage(MsgType.Error, "Unknown command").ToString());
                }
            }
            catch
            {
                
            }
            finally
            {
                lock (_clients)
                {
                    _clients.Remove(client);
                }
                tcpClient.Close();
            }
        }

        public async Task BroadcastAsync(MsgType type, string content)
        {
            if (type == MsgType.Passwd)
                throw new InvalidOperationException("Cannot broadcast 'Passwd' type");

            var message = new SrvMessage(type, content).ToString();
            var tasks = new List<Task>();

            lock (_clients)
            {
                foreach (var client in _clients.Where(c => c.IsAuthenticated))
                {
                    tasks.Add(SendAsync(client, message));
                }
            }

            await Task.WhenAll(tasks);
        }

        public async Task SendToAsync(ClientContext client, MsgType type, string content)
        {
            if (type == MsgType.Passwd)
                throw new InvalidOperationException("Cannot send 'Passwd' type");

            if (!client.IsAuthenticated)
                return;

            var writer = new StreamWriter(client.Stream, new UTF8Encoding(false)) { AutoFlush = true };
            await SendAsync(client, new SrvMessage(type, content).ToString());
        }

        private async Task SendAsync(ClientContext client, string message)
        {
            var writer = new StreamWriter(client.Stream, new UTF8Encoding(false)) { AutoFlush = true };
            await writer.WriteLineAsync(message);
        }

        private static string? GetLocalIP()
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up)
                    continue;

                var ipProps = ni.GetIPProperties();
                foreach (var ua in ipProps.UnicastAddresses)
                {
                    if (ua.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ua.Address))
                        return ua.Address.ToString();
                }
            }
            return null;
        }

        private static string ComputeSha256(string input)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
