using MultiDebug.Server;

namespace MultiDebug.Server_exe
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("MultiDebug Server");

            if (args.Length < 2)
            {
                Console.WriteLine("Usage: MultiDebug.Server.exe <debug|release> <port>");
                return;
            }

            var modeArg = args[0].ToLower();
            var isDebug = modeArg == "debug";

            if (!int.TryParse(args[1], out var port))
            {
                Console.WriteLine("Invalid port number.");
                return;
            }

            string? password = null;
            if (!isDebug)
            {
                Console.Write("Enter server password: ");
                password = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(password))
                {
                    Console.WriteLine("Password is required in release mode.");
                    return;
                }
            }

            var server = new MDServer(port, isDebug, true, password);

            server.RegisterCommand("ping", async (ctx, args) =>
            {
                await server.SendToAsync(ctx, MsgType.Error, "pong");
            });

            server.RegisterCommand("echo", async (ctx, args) =>
            {
                var message = string.Join(' ', args);
                await server.BroadcastAsync(MsgType.Info, message);
            });
            await server.StartAsync(port);

        }
    }
}
