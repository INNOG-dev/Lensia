using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class NetworkChannel
{

    public List<System.Type> packets = new List<System.Type>();

    private byte channelId;


    public NetworkChannel(byte channelId)
    {
        this.channelId = channelId;
    }

    public byte getChannelId()
    {
        return channelId;
    }


    public bool registerPacket(System.Type packet)
    {
        if (!packet.GetType().IsInstanceOfType(typeof(NMSG)))
        {
            throw new System.Exception("Type of packet must be NMSG");
        }
        if (packets.Count >= 256)
        {
            throw new System.Exception("Packets count exceed limit of 255");
        }
        if (packets.Contains(packet))
        {
            throw new System.Exception("Packet already registered");
        }
        packets.Add(packet);
        return true;
    }


}

