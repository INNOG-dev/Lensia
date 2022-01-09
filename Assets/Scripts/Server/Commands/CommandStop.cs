using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandStop : CommandBase
{
    public CommandStop(string cmdName, string cmdDescription, string[] args) : base(cmdName, cmdDescription, args) { }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.OPERATOR;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if (cmdName == "stop")
        {
            if (!senderHasPermission(sender))
            {
                sender.sendMessage("<color=red>Vous navez pas la permission</color>");
                return true;
            }

            if (args.Length > 0)
            {
                Debug.Log("Please respect command arguments /help to see list of commands");
            }
            else
            {
                Debug.Log("Stopping server...");
                Debug.Log("Server Stopped!");
            }
            return true;
        }
        return false;
    }

}
