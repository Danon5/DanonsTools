using System;

namespace DanonsTools.CommandConsoleLayer
{
    public interface IConsoleCommandOverload
    {
        public Type[] ParameterTypes { get; }
        public string Description { get; }

        public void Execute(params string[] parameters);
    }
}