using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSender : ICommandSender
{

    private uint entityId;

    public PlayerSender(uint entityId)
    {
        this.entityId = entityId;
    }

    public Player getPlayer()
    {
       return (Player) Server.getServer().getWorldManager().getEntity(entityId);
    }

    public void sendMessage(string message)
    {
        if(!NetworkSide.isRemote())
          Server.getServer().getChatManager().sendMessageToPlayer(message, getPlayer().getNetworkId());
    }
}
