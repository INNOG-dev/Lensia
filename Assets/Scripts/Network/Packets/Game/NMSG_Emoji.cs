using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_Emoji : NMSG
{

    private uint entityId;

    private uint emojiId;

    public static NMSG_Emoji sendEmoji(uint emojiId)
    {
        NMSG_Emoji packet = new NMSG_Emoji();
        packet.emojiId = emojiId;
        return packet;
    }

    /// <summary>
    //
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="emojiId"></param>
    /// 
    /// sender from server to client
    /// 
    /// <returns></returns>
    public static NMSG_Emoji syncEmoji(Entity sender, uint emojiId)
    {
        NMSG_Emoji packet = new NMSG_Emoji();
        packet.emojiId = emojiId;
        packet.entityId = sender.getEntityId();
        return packet;
    }

    public override void HandleClient(Client client)
    {
        World world = client.GetNetworkUser().getPlayer().getCurrentWorld();
        if (world == null) return;

        if(world.getEntitiesInWorld().ContainsKey(entityId))
        {
            world.getEntitiesInWorld()[entityId].displayEmoji(emojiId);
        }
    }

    public override void HandleServer(Server server, int networkId)
    {
        server.users[networkId].getPlayer().displayEmoji(emojiId);
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        if (NetworkSide.isRemote())
        {
            emojiId = reader.ReadUInt32();
            entityId = reader.ReadUInt32();
        }
        else
        {
            emojiId = reader.ReadUInt32();
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        if(NetworkSide.isRemote())
        {
            writer.Write(emojiId);
        }
        else
        {
            writer.Write(emojiId);
            writer.Write(entityId);
        }
    }
}
