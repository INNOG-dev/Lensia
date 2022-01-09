using System.IO;
using UnityEngine;

public class NMSG_Chat : NMSG
{

    private string message;

    private uint messageSenderEntityId;

    private bool systemMessage;

    public static NMSG_Chat sendMessage(string message)
    {
        NMSG_Chat packet = new NMSG_Chat();
        packet.message = message;
        packet.systemMessage = true;
        return packet;
    }

    public static NMSG_Chat sendMessage(uint entityId, string message)
    {
        NMSG_Chat packet = new NMSG_Chat();
        packet.messageSenderEntityId = entityId;
        packet.message = message;
        packet.systemMessage = false;
        return packet;
    }


    public override void HandleClient(Client client)
    {
        InGameUI.ChatContainer chatContainer = (InGameUI.ChatContainer) InGameUI.INSTANCE.getInterface(1);

        string senderUsername;
        if(systemMessage)
        {
            senderUsername = "<color=red>[Server]</color>";
        }
        else
        {
            Player player = client.GetNetworkUser().getPlayer().getCurrentWorld().getPlayersInWorld()[messageSenderEntityId];

            senderUsername = player.getName();

            if(player.getCurrentChatBubble() != null)
            {
                player.getCurrentChatBubble().Destroy();
            }

            player.displayChatBubble(new ChatBubble(player, message));
        }

        chatContainer.addMessageToContainer(senderUsername, message);
    }

    public override void HandleServer(Server server, int networkId)
    {
        PlayerSender sender = new PlayerSender(server.users[networkId].getPlayer().getEntityId());
        if (sender.getPlayer().isAfk())
        {
            sender.getPlayer().setAfk(false);
        }
        server.getChatManager().HandleMessage(sender, message);
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        if(!NetworkSide.isRemote())
        {
            message = reader.ReadString();
        }
        else
        {
            message = reader.ReadString();
            systemMessage = reader.ReadBoolean();
            if(!systemMessage)
            {
                messageSenderEntityId = reader.ReadUInt32();
            }
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        if(NetworkSide.isRemote())
        {
            writer.Write(message);
        }
        else
        {
            writer.Write(message);
            writer.Write(systemMessage);
            if(!systemMessage)
            {
                writer.Write(messageSenderEntityId);
            }
        }
    }
}
