using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ConfidentialityData : INetworkSerializable
{

    private bool acceptFriendRequest;

    private bool acceptNewslestter;

    private bool alwaysOffline;

    private bool mpOnlyFriends;

    public void setAcceptFriendRequests(bool state)
    {
        acceptFriendRequest = state;
    }

    public bool getAcceptFriendRequests()
    {
        return acceptFriendRequest;
    }

    public void registerNewsletter(bool state)
    {
        acceptNewslestter = state;
    }

    public bool isRegisterNewsletter()
    {
        return acceptNewslestter;
    }

    public void setAlwaysOffline(bool state)
    {
        alwaysOffline = state;
    }

    public bool isAlwaysOffline()
    {
        return alwaysOffline;
    }

    public void setMpOnlyFriend(bool state)
    {
        mpOnlyFriends = state;
    }

    public bool onlyFriendCanMp()
    {
        return mpOnlyFriends;
    }

    public void WriteToStream(BinaryWriter writer)
    {
        writer.Write(acceptFriendRequest);
        writer.Write(acceptNewslestter);
        writer.Write(alwaysOffline);
        writer.Write(mpOnlyFriends);
    }

    public void ReadFromStream(BinaryReader reader)
    {
        acceptFriendRequest = reader.ReadBoolean();
        acceptNewslestter = reader.ReadBoolean();
        alwaysOffline = reader.ReadBoolean();
        mpOnlyFriends = reader.ReadBoolean();
    }

    public int getSize()
    {
        return 4;
    }
}
