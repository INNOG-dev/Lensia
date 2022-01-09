using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CommandRegistry
{
    public static List<CommandBase> registeredCommands = new List<CommandBase>();

    public static void registerCommand(CommandBase command)
    {
        if (registeredCommands.Contains(command))
        {
            throw new System.Exception("Command already registered");
        }
        registeredCommands.Add(command);
        Debug.Log("command " + command.getCommandName() + " registered");
    }

}
