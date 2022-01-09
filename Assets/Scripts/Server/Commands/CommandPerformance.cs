using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandPerformance : CommandBase
{
    public CommandPerformance(string cmdName, string cmdDescription, string[] displayArgs) : base(cmdName, cmdDescription, displayArgs)
    {
       
    }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.OPERATOR;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if(cmdName == "performance")
        {
            Server server = Server.getServer();
            sender.sendMessage("<color=red>Server stats :</color>");
            sender.sendMessage("<color=green>Entities count : " + server.getWorldManager().getEntityCount());
            sender.sendMessage("<color=green>Loaded world count : " + server.getWorldManager().getLoadedWorlds().Count);
            sender.sendMessage("<color=green>TPS : " + server.getTicksPerSeconds() + "/" + Server.fixedUpdateTicks);
            return true;
        }

        return false;
    }

}
