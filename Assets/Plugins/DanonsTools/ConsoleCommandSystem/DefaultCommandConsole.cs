using System;
using System.Collections.Generic;

namespace DanonsTools.ConsoleCommandSystem
{
    public sealed class DefaultCommandConsole : ICommandConsole
    {
        public Action<string, ConsoleLogType> ConsoleLogEvent { get; set; }
        public Dictionary<string, IConsoleCommand> RegisteredCommands { get; } = new();

        public void RegisterCommand(in IConsoleCommand command)
        {
            RegisteredCommands.TryAdd(command.Keyword.ToLower(), command);
        }

        public void ProcessInput(in string input)
        {
            if (input.Equals(string.Empty)) return;

            var parsedInput = input.ToLower();
            
            var splitInput = parsedInput.Split(' ');

            if (!RegisteredCommands.TryGetValue(splitInput[0], out var command))
            {
                Log($"Unknown command '{input.Split(' ')[0]}'.", ConsoleLogType.Error);
                return;
            }

            ConsoleUtils.TryFindAndExecuteOverload(splitInput, command.Overloads, this);
        }

        public void Log(in string message, in ConsoleLogType logType)
        {
            ConsoleLogEvent?.Invoke(message, logType);
        }
    }
}
