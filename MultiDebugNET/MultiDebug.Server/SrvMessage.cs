namespace MultiDebug.Server
{
    public class SrvMessage
    {
        public MsgType Type { get; }
        public string Content { get; }

        public SrvMessage(MsgType type, string content)
        {
            Type = type;
            Content = content;
        }

        public override string ToString() => $"{Type}:{Content}";

    }
}
