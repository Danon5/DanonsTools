using System;
using System.Collections.Generic;

namespace DanonsTools.CommandConsoleLayer
{
    public interface ICommandConsole
    {
        public Action<string, ConsoleLogType> ConsoleLogEvent { get; set; }
        public Dictionary<string, IConsoleCommand> RegisteredCommands { get; }

        public void RegisterCommand(in IConsoleCommand command);
        public void ProcessInput(in string input);
        public void Log(in string message, in ConsoleLogType logType);
    }
}