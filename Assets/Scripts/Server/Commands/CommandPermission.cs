using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandPermission : CommandBase
{
    public CommandPermission(string cmdName, string cmdDescription, string[] args) : base(cmdName, cmdDescription, args) { }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if (cmdName == "permission")
        {
            Server server = Server.getServer();
   
            if (!senderHasPermission(sender))
            {
                sender.sendMessage("<color=red>Vous n'avez pas la permission</color>");
                return true;
            }
            
            if(args.Length >= 3)
            {
                NetworkUser user = server.getUserFromUsername(args[1]);
                
                if(user == null)
                {
                    sender.sendMessage("<color=red>Utilisateur introuvable</color>");
                    return true;
                }

                int level;
                
                if(!int.TryParse(args[2], out level))
                {
                    sender.sendMessage("<color=red>Le niveau doit être un nombre entier</color>");
                    return true;
                }

                user.getPermission().setUserPermission((UserPermission.PermissionsLevel)level);
                user.setDirty(true);

                sender.sendMessage("<color=green>L'utilisateur <color=orange>" + args[1] + "</color> est maintenant <color=orange>" + user.getPermission().getPermissionLevel() + "</color></color>");
            }
            else
            {
                sender.sendMessage("<color=red>/permission set <Username> <level></color>");
            }
            return true;
        }

        return false;
    }

    public void displayHelp(ICommandSender sender)
    {
        string displayText = "Liste des commandes : \n";

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

            if (userPermission >= cmd.getCommandPermissionsLevel())
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
        return UserPermission.PermissionsLevel.OPERATOR;
    }
}
