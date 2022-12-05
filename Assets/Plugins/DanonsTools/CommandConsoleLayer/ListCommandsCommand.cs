using System;

namespace DanonsTools.CommandConsoleLayer
{
    public sealed class ListCommandsCommand : IConsoleCommand
    {
        public string Keyword => _KEYWORD;
        public string Description => _DESCRIPTION;
        public IConsoleCommandOverload[] Overloads { get; }
        
        private const string _KEYWORD = "listcommands";
        private const string _DESCRIPTION = "Displays a list of all registered commands.";

        public ListCommandsCommand(in ICommandConsole console)
        {
            Overloads = new IConsoleCommandOverload[]
            {
                new DefaultOverload(console)
            };
        }

        private sealed class DefaultOverload : IConsoleCommandOverload
        {
            public Type[] ParameterTypes => Type.EmptyTypes;
            public string Description => $"{_KEYWORD} - {_DESCRIPTION}";

            private readonly ICommandConsole _console;
            
            public DefaultOverload(in ICommandConsole console)
            {
                _console = console;
            }
            
            public void Execute(params string[] parameters)
            {
                _console.Log("All registered commands:", ConsoleLogType.Message);
            
                foreach (var command in _console.RegisteredCommands.Values)
                    _console.Log($"     {command.Keyword} - {command.Description}", ConsoleLogType.Message);
            }
        }
    }
}