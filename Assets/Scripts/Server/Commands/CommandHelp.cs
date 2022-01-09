using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandHelp : CommandBase
{
    public CommandHelp(string cmdName, string cmdDescription, string[] args) : base(cmdName,cmdDescription,args) { }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if(cmdName == "help")
        {
            displayHelp(sender);
            return true;
        }

        return false;
    }

    public void displayHelp(ICommandSender sender)
    {
        string displayText = "Liste des commandes :";

        UserPermission.PermissionsLevel userPermission = UserPermission.PermissionsLevel.OPERATOR;

        PlayerSender pSender = null;
        Server server = Server.getServer();

        if (sender is PlayerSender)
        {
            pSender = (PlayerSender)sender;
            
            NetworkUser user = server.users[pSender.getPlayer().getNetworkId()];
            userPermission = user.getPermission().getPermissionLevel();
        }

        foreach (CommandBase cmd in CommandRegistry.registeredCommands)
        {

            if(userPermission >= cmd.getCommandPermissionsLevel())
            {
                displayText += '\n';

                displayText += "/" + cmd.getCommandName();

                if (cmd.getArgs() != null)
                {
                    foreach (string arg in cmd.getArgs())
                    {
                        displayText += " " + arg;
                    }

                    if (getDescription() != null) displayText += " - " + cmd.getDescription();

                    
                }
            }
        }

        if (pSender != null)
        {
            server.getChatManager().sendMessageToPlayer(displayText, pSender.getPlayer().getNetworkId());
        }
        else
        {
            Debug.Log(displayText);
        }
    }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.NORMAL;
    }
}
