namespace MultiDebug.Client
{
    public class SrvMessage
    {
        public MsgType Type { get; set; }
        public string Content { get; set; } = string.Empty;

        public static bool TryParse(string line, out SrvMessage message)
        {
            message = new SrvMessage();
            var idx = line.IndexOf(':');
            if (idx < 0) return false;

            var typeStr = line[..idx];
            var content = line[(idx + 1)..];

            if (!Enum.TryParse(typeStr, out MsgType type)) return false;

            message.Type = type;
            message.Content = content;
            return true;
        }

        public override string ToString() => $"{Type}:{Content}";
    }
}