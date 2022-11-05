using DanonsTools.ConsoleCommandSystem;
using UnityEngine;

namespace TestScripts
{
    public sealed class ConsoleCommandTester : MonoBehaviour
    {
        private ICommandConsole _console;

        private void Start()
        {
            _console = new DefaultCommandConsole();

            _console.ConsoleLogEvent += (s, type) => Debug.Log(s);

            _console.RegisterCommand(new HelpCommand(_console));
            _console.RegisterCommand(new ListCommandsCommand(_console));

            _console.ProcessInput("help");
        }
    }
}
