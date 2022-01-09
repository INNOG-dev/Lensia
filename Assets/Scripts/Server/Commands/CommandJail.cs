using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandJail : CommandBase
{

    public CommandJail(string cmdName, string cmdDescription, string[] args) : base(cmdName, cmdDescription, args) { }

    public override UserPermission.PermissionsLevel getCommandPermissionsLevel()
    {
        return UserPermission.PermissionsLevel.MODERATOR;
    }

    public override bool onCommand(string cmdName, ICommandSender sender, string[] args)
    {
        if(cmdName == "jail")
        {

            if(!senderHasPermission(sender))
            {
                sender.sendMessage("<color=red>Vous n'avez pas la permission</color>");
                return true;
            }

            if (args.Length >= 3)
            {
                Server server = Server.getServer();

                NetworkUser user = server.getUserFromUsername(args[0]);

                if(user == null)
                {
                    sender.sendMessage("<color=red>Utilisateur introuvable</color>");
                    return true;
                }

                int timeInSeconds;
                if(!int.TryParse(args[1], out timeInSeconds))
                {
                    sender.sendMessage("<color=red>Le temps doit être un entier</color>");
                    return true;
                }

                string reason = "";

                for(int i = 2; i < args.Length; i++)
                {
                    reason += " " + args[i];
                }

                user.getPlayer().jail(timeInSeconds);
                sender.sendMessage("<color=green>Vous avez mis <color=purple>" + user.getAccountData().getUsername() + " <color=green>en prison pour <color=purple>" + DateUtils.formatFromSeconds("HH:mm:ss", timeInSeconds) + " <color=blue>pour : " + reason + "</color></color></color></color></color>");
                user.getPlayer().sendMessage("<color=green>Vous avez été mis en prison pour <color=purple>" + DateUtils.formatFromSeconds("HH:mm:ss", timeInSeconds) + " <color=blue> raison : " + reason + "</color></color></color>");
                //ban

            }
            else
            {
                sender.sendMessage("<color=red>/jail <Username> <Temps en secondes> <Raison></color>");
            }

            return true;
        }

        return false;
    }
}
