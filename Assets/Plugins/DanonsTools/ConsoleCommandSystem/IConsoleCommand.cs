namespace DanonsTools.ConsoleCommandSystem
{
    public interface IConsoleCommand
    {
        public string Keyword { get; }
        public string Description { get; }
        public IConsoleCommandOverload[] Overloads { get; }
    }
}