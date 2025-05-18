using Godot;
using System;
using MultiDebug.Client;

[Tool]
public partial class Client : Node
{
    internal Control _dockPanel;

    public void Init(Control DockPanel)
    {
        _dockPanel = DockPanel;
    }

    public void Connect()
    {
        var ip = _dockPanel.GetNode<LineEdit>("IPInput").Text;
        var port = Convert.ToInt32(_dockPanel.GetNode<LineEdit>("PortInput").Text);
        var logger = new Logger(_dockPanel);
        MDClient.Connect(logger, ip, port);
    }

    public void Disconnect()
    {
        var logger = new Logger(_dockPanel);
        MDClient.Disconnect(logger);
    }

    public void Send()
    {
        var input = _dockPanel.GetNode<LineEdit>("CommandInput");
        var logger = new Logger(_dockPanel);
        MDClient.Send(logger, input.Text);
    }
}

public class Logger : ILogger
{
    private readonly Control _dockPanel;

    public Logger(Control dockPanel)
    {
        _dockPanel = dockPanel;
    }

    public void Log(MultiDebug.Client.MsgType Type, string Content)
    {
        var log = _dockPanel.GetNode<TextEdit>("LogView");
        log.Text += $"\n{Content}";
        log.ScrollVertical = log.GetLineCount();
    }

    public void LogClear()
    {
        var log = _dockPanel.GetNode<TextEdit>("LogView");
        log.Text = "";
    }

    public void onConnect()
    {
        _dockPanel.GetNode<Button>("ConnectButton").Disabled = true;
        _dockPanel.GetNode<Button>("DisconnectButton").Disabled = false;
        _dockPanel.GetNode<Button>("SendButton").Disabled = false;
        _dockPanel.GetNode<LineEdit>("CommandInput").Editable = true;
    }

    public void onDisconnect()
    {
        _dockPanel.GetNode<Button>("ConnectButton").Disabled = false;
        _dockPanel.GetNode<Button>("DisconnectButton").Disabled = true;
        _dockPanel.GetNode<Button>("SendButton").Disabled = true;
        _dockPanel.GetNode<LineEdit>("CommandInput").Editable = false;
    }

    public void ClearInputLine()
    {
        var input = _dockPanel.GetNode<LineEdit>("CommandInput");
        input.Text = "";
    }
}
