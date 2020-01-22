using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Scripting;

public static class DeveloperCommands
{
    static Dictionary<string, Command> _commands;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void Init()
    {
        _commands = new Dictionary<string, Command>();

        var methods = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).SelectMany(t => t.GetMethods())
            .Select(m => (method: m, data: m.GetCustomAttribute<CommandAttribute>())).Where(m => m.data != null);

        foreach (var (method, data) in methods)
        {
            var cmd = new Command(method);
            foreach (var alias in data.Aliases)
            {
                _commands.Add(alias.ToLower(), cmd);
            }
        }
    }

    public static void Execute(string cmd)
    {
        var data = cmd.Trim().Split("\"".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).SelectMany((s, i) =>
                i % 2 == 0 ? s.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) : new[] {s})
            .ToArray();
        var cmdName = data[0].ToLower();
        if (!_commands.ContainsKey(cmdName))
        {
            Debug.LogError("Invalid command");
            return;
        }
        _commands[cmdName].Execute(data.Skip(1).ToArray());
    }
}

[AttributeUsage(AttributeTargets.Method, Inherited = false)]
public sealed class CommandAttribute : PreserveAttribute
{
    public string[] Aliases { get; }

    public CommandAttribute(params string[] aliases)
    {
        Aliases = aliases;
    }
}

internal struct Command
{
    public MethodInfo cmdMethod;
    public Type[] cmdArgTypes;
    
    public Command(MethodInfo method)
    {
        cmdMethod = method;
        cmdArgTypes = method.GetParameters().Select(p => p.ParameterType).ToArray();
    }

    public void Execute(string[] cmdArgs)
    {
        var argTypes = cmdArgTypes;
        if (cmdArgs.Length != cmdArgTypes.Length)
        {
            Debug.LogError($"Invalid number of arguments supplied to command. Expected {cmdArgTypes.Length}, got {cmdArgs.Length}");
            return;
        }

        var args = cmdArgs.Select((a, i) => Convert.ChangeType(a, argTypes[i])).ToArray();
        cmdMethod.Invoke(null, args);
    }
}