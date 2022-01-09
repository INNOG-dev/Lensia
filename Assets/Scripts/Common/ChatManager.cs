using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChatManager
{

    public ChatManager()
    {

    }

    public void sendMessageToServer(string message)
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(NMSG_Chat.sendMessage(message), client.reliableChannel, client.gameChannel.getChannelId());
        }
    }

    public void sendMessageToPlayer(string message, int receiverNetworkId)
    {
        if(!NetworkSide.isRemote())
        {
            Server server = Server.getServer();

            server.sendToClient(NMSG_Chat.sendMessage(message), server.reliableChannel, server.gameChannel.getChannelId(), receiverNetworkId);
        }
    }

    //Send to everyone
    public void sendMessageAll(int senderNetworkId, string message)
    {
        Server server = Server.getServer();
        NetworkUser user = server.users[senderNetworkId];

        server.broadcast(NMSG_Chat.sendMessage(user.getPlayer().getEntityId(), message), server.reliableChannel, server.gameChannel.getChannelId());
    }

    public void sendMessageToWorld(int senderNetworkId, string message)
    {
        Server server = Server.getServer();
        NetworkUser user = server.users[senderNetworkId];

        World world = user.getPlayer().getCurrentWorld();
        server.broadcastWorld (NMSG_Chat.sendMessage(user.getPlayer().getEntityId(), message), world.getWorldId(), server.reliableChannel, server.gameChannel.getChannelId());
    }

    public void HandleMessage(ICommandSender sender, string message)
    {
        if (message.StartsWith("/"))
        {
            string[] splitedStr = message.Split(' ');

            string cmdName = splitedStr[0].Substring(1);

            string[] args = new string[splitedStr.Length - 1];

            int i = 0;

            bool commandFound = false;

            foreach (string arg in splitedStr)
            {
                if (!arg.StartsWith("/"))
                {
                    args[i] = arg;
                    i++;
                }
            }

            foreach (CommandBase cb in CommandRegistry.registeredCommands)
            {
                if (cb.onCommand(cmdName, sender, args))
                {
                    commandFound = true;
                }
            }

            if (commandFound) return;

           
           sender.sendMessage("Commande inconnue merci d'utiliser /help pour voir la liste des commandes");
            
        }
        else
        {
            if (message.Length > ServerSettings.maxMessageLenght) return;

            if (sender is PlayerSender)
            {
                Player player = ((PlayerSender)sender).getPlayer();

                NetworkUser user = Server.getServer().users[player.getNetworkId()];

                if (user.getPermission().getPermissionLevel() == UserPermission.PermissionsLevel.NORMAL)
                {
                    message = Regex.Replace(message, "<.*?>", string.Empty);
                }

                sendMessageToWorld(player.getNetworkId(),message);
            }
        }
    }

}
