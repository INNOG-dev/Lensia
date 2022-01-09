using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CommandPrivateMessage : CommandBase
{

    public CommandPrivateMessage(string cmdName, string cmdDescription, string[] displayArgs) : base(cmdName, cmdDescription, displayArgs) { }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.NORMAL;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if (cmdName == "mp")
        {
            if (sender is PlayerSender)
            {
                PlayerSender playerSender = (PlayerSender)sender;
                Server server = Server.getServer();

                if (args.Length == 0)
                {
                    sender.sendMessage("<color=red>Mauvais syntaxe de la commande");
                    return true;
                }
                else if (args.Length == 1)
                {
                    sender.sendMessage("<color=red>Vous devez entrer un message");
                    return true;
                }
                else if (args.Length >= 2)
                {
                    NetworkUser receiver = Server.getServer().getUserFromUsername(args[0]);
                    

                    if (receiver == null)
                    {
                        sender.sendMessage("<color=red>Joueur introuvable"); //utilisateur introuvable;
                        return true;
                    }

                    string message = "";
                    for (int i = 1; i < args.Length; i++)
                    {
                        message += args[i] + " ";
                    }
                    NetworkUser senderUser = server.users[playerSender.getPlayer().getNetworkId()];

                    receiver.getPlayer().sendMessage("<color=orange>De " +  senderUser.getAccountData().getUsername() +  ": " +  message + "</color>");
                    sender.sendMessage("<color=orange>" + senderUser.getAccountData().getUsername() + " -> " + receiver.getAccountData().getUsername() + " : " + message + "</color>");
                    return true;
                }
            }
        }
        return false;
    }
}
