using Godot;
using MultiDebug.Server;

public partial class Server : Node
{
	public static Server Instance { get; private set; }

	private MDServer server;

	[Export] public int Port = (int)ProjectSettings.GetSetting("multidebug/server/port", 5000);
	[Export] public bool DebugMode = (bool)ProjectSettings.GetSetting("multidebug/server/debug_mode", false);
	[Export] public bool Enable = (bool)ProjectSettings.GetSetting("multidebug/server/enable", true);
	[Export] public string Password = (string)ProjectSettings.GetSetting("multidebug/server/password", "admin");

	public override async void _Ready()
	{
		if (Instance != null)
		{
			QueueFree();
			return;
		}

		Instance = this;
		server = new MDServer(Port, DebugMode, Enable, DebugMode ? null : Password);

		// Test commands
		server.RegisterCommand("ping", async (ctx, args) =>
		{
			await server.SendToAsync(ctx, MultiDebug.Server.MsgType.Info, "pong");
		});

		server.RegisterCommand("echo", async (ctx, args) =>
		{
			var msg = string.Join(" ", args);
			await server.SendToAsync(ctx, MultiDebug.Server.MsgType.Info, msg);
		});

		server.RegisterCommand("broadcast", async (ctx, args) =>
		{
			var msg = string.Join(" ", args);
			await server.BroadcastAsync(MultiDebug.Server.MsgType.Info, msg);
		});

		await server.StartAsync(Port);
		GD.Print("MDServer Started!");
	}

	public async void Broadcast(MultiDebug.Server.MsgType type, string message)
	{
		if (server != null)
			await server.BroadcastAsync(type, message);
	}
}
