using System;
using System.Collections.Generic;
using System.Linq;

namespace DanonsTools.ConsoleCommandSystem
{
    public static class ConsoleUtils
    {
        public static bool TryFindAndExecuteOverload(in string[] input, in IConsoleCommandOverload[] overloads, in ICommandConsole console)
        {
            var parameters = input.Skip(1).ToArray();
            
            var parameterTypes = ParseParametersIntoTypes(parameters);

            if (!TryGetOverloadWithParameterTypes(parameterTypes, overloads, out var overload))
            {
                console.Log($"Unknown overload for command '{input[0]}'.", ConsoleLogType.Error);
                return false;
            }
            
            overload.Execute(parameters);
            return true;
        }
        
        public static Type[] ParseParametersIntoTypes(in string[] parameters)
        {
            var types = new Type[parameters.Length];

            for (var i = 0; i < parameters.Length; i++)
            {
                if (int.TryParse(parameters[i], out var intValue))
                    types[i] = intValue.GetType();
                else if (float.TryParse(parameters[i], out var floatValue))
                    types[i] = floatValue.GetType();
                else if (bool.TryParse(parameters[i], out var boolValue))
                    types[i] = boolValue.GetType();
                else
                    types[i] = typeof(string);
            }

            return types;
        }
        
        public static bool TryGetOverloadWithParameterTypes(in Type[] parameterTypes, 
            in IEnumerable<IConsoleCommandOverload> overloads, out IConsoleCommandOverload overload)
        {
            foreach (var o in overloads)
            {
                if (o.ParameterTypes.Length != parameterTypes.Length) continue;

                var paramLength = o.ParameterTypes.Length;

                if (paramLength == 0)
                {
                    overload = o;
                    return true;
                }
                
                for (var i = 0; i < paramLength; i++)
                {
                    var desiredParam = parameterTypes[i];
                    var param = o.ParameterTypes[i];
                    
                    if (param != desiredParam) break;
                    if (i != paramLength - 1 || param != desiredParam) continue;
                    
                    overload = o;
                    return true;
                }
            }

            overload = default;
            return false;
        }
    }
}