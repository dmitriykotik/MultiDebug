using System.Net.Sockets;

namespace MultiDebug.Server
{
    public class ClientContext
    {
        public TcpClient TcpClient { get; }
        public NetworkStream Stream => TcpClient.GetStream();
        public bool IsAuthenticated { get; set; } = false;

        public ClientContext(TcpClient client)
        {
            TcpClient = client;
        }
    }
}
