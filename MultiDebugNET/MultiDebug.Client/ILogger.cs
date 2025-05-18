using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiDebug.Client
{
    public interface ILogger
    {
        public void Log(MsgType Type, string Content);
        public void LogClear();
        public void onConnect();
        public void onDisconnect();
        public void ClearInputLine();
    }
}
