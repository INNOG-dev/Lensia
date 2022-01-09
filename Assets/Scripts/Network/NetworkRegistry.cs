using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkRegistry
{
    private static Dictionary<byte, NetworkChannel> channels = new Dictionary<byte, NetworkChannel>();

    private NetworkRegistry() { }

    public static NetworkChannel getChannel(byte channelId)
    {

        if (!channels.ContainsKey(channelId))
        {
            Debug.Log("Channel doesn't exist");
            return null;
        }

        return channels[channelId];
    }

    public static NetworkChannel registerChannel(byte channelId)
    {
        NetworkChannel channel = new NetworkChannel(channelId);
        channels.Add(channelId, channel);
        return channel;
    }
}
