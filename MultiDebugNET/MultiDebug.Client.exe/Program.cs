using MultiDebug.Client;

namespace MultiDebug.Client_exe
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MultiDebug Client");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MultiDebug.Client.exe <host> <port>");
                return;
            }

            var host = args[0];
            if (!int.TryParse(args[1], out var port))
            {
                Console.WriteLine("Invalid port number.");
                return;
            }

            try
            {
                MDClient.Connect(new Logger(), host, port);

                while (true)
                {
                    var input = Console.ReadLine();
                    MDClient.Send(new Logger(), input);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Connection failed: {ex.Message}");
            }
        }
    }

    internal class Logger : ILogger
    {
        public void Log(MsgType Type, string Content) 
        {
            switch (Type)
            {
                case MsgType.Info:
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine(Content);
                    Console.ResetColor();
                    break;
                case MsgType.Debug:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    Console.WriteLine(Content);
                    Console.ResetColor();
                    break;
                case MsgType.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(Content);
                    Console.ResetColor();
                    break;
                case MsgType.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(Content);
                    Console.ResetColor();
                    break;
                default:
                    Console.WriteLine(Content);
                    break;
            }
        }
        public void LogClear() 
        {
            Console.Clear();
        }
        public void onConnect() { }
        public void onDisconnect() { }
        public void ClearInputLine() { }
    }
}
