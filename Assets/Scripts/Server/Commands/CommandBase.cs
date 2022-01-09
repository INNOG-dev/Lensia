using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CommandBase
{

    private string cmdName = "";

    private string cmdDescription = "";

    private string[] args;

    public CommandBase(string cmdName, string cmdDescription, string[] displayArgs)
    {
        this.cmdName = cmdName;
        this.cmdDescription = cmdDescription;
        args = displayArgs;
    }

    public abstract bool onCommand(string cmdName, ICommandSender sender, string[] args);

    public string getCommandName()
    {
        return cmdName;
    }

    public string getDescription()
    {
        return cmdDescription;
    }

    public string[] getArgs()
    {
        return this.args;
    }

    public bool senderHasPermission(ICommandSender sender)
    {
        if(sender is PlayerSender)
        {
            PlayerSender pSender = (PlayerSender)sender;

            Server server = Server.getServer();

            if (server.users[pSender.getPlayer().getNetworkId()].getPermission().getPermissionLevel() >= getCommandPermissionsLevel()) return true;

            return false;
        }

        return true;
    }

    public abstract UserPermission.PermissionsLevel getCommandPermissionsLevel();

}
