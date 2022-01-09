using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandDodo : CommandBase
{

    public CommandDodo(string cmdName, string cmdDescription, string[] displayArgs) : base(cmdName, cmdDescription, displayArgs)
    {

    }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        //�a c'est pour d�finir la permission de la commande
        return UserPermission.PermissionsLevel.NORMAL;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if(cmdName == "dodo")
        {
            if(sender is PlayerSender)
            {
                PlayerSender pSender = (PlayerSender)sender;
                pSender.getPlayer().setAfk(true);
                pSender.sendMessage("<color=red>Faites de bon r�ve</color>");
            }

            return true;
        }

        return false;
    }
}
