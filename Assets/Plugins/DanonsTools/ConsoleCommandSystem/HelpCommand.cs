using System;

namespace DanonsTools.ConsoleCommandSystem
{
    public sealed class HelpCommand : IConsoleCommand
    {
        public string Keyword => _KEYWORD;
        public string Description => _DESCRIPTION;
        public IConsoleCommandOverload[] Overloads { get; }

        private const string _KEYWORD = "help";
        private const string _DESCRIPTION = "Provides documentation on commands.";

        public HelpCommand(in ICommandConsole console)
        {
            Overloads = new IConsoleCommandOverload[]
            {
                new GeneralHelpOverload(console),
                new SpecificHelpOverload(console)
            };
        }

        private sealed class GeneralHelpOverload : IConsoleCommandOverload
        {
            public Type[] ParameterTypes => Type.EmptyTypes;
            public string Description => $"{_KEYWORD} - {_DESCRIPTION}";

            private readonly ICommandConsole _console;
            
            public GeneralHelpOverload(in ICommandConsole console)
            {
                _console = console;
            }
            
            public void Execute(params string[] parameters)
            {
                _console.Log("Type 'listcommands' for a list of all commands.", ConsoleLogType.Message);
                _console.Log("Type 'help <commandName>' for more information about the usage of a specific command.", ConsoleLogType.Message);
            }
        }

        private sealed class SpecificHelpOverload : IConsoleCommandOverload
        {
            public Type[] ParameterTypes { get; } = 
            {
                typeof(string)
            };

            public string Description => $"{_KEYWORD} <commandName> - Provides more detailed documentation about the specified command.";

            private readonly ICommandConsole _console;

            public SpecificHelpOverload(in ICommandConsole console)
            {
                _console = console;
            }
            
            public void Execute(params string[] parameters)
            {
                if (_console.RegisteredCommands.TryGetValue(parameters[0], out var command))
                {
                    _console.Log($"{parameters[0]} usages:", ConsoleLogType.Message);
                    
                    foreach (var overload in command.Overloads)
                        _console.Log($"     {overload.Description}", ConsoleLogType.Message);
                }
            }
        }
    }
}