using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_SyncAccountData : CallbackNMSG
{

    private NetworkUser user;

    public NMSG_SyncAccountData() : base(null)
    {

    }

    public NMSG_SyncAccountData(System.Action callback) : base(null)
    {

    }

    public NMSG_SyncAccountData(NetworkUser user) : base(null)
    {
        this.user = user;
    }

    public override void HandleClient(Client client)
    {
        NetworkUser user = client.GetNetworkUser();

        user.setAccountData(this.user.getAccountData());
        user.setConfidentialityData(this.user.getConfidentialityData());
    }

    public override void HandleServer(Server server, int networkId) 
    {
        this.user = server.users[networkId];
        server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        base.ReadFromStream(reader);
        if (NetworkSide.isRemote())
        {
            this.user = new NetworkUser(-1);
            user.ReadFromStream(reader);
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        base.WriteToStream(writer);
        if(!NetworkSide.isRemote()) user.WriteToStream(writer);
    }
}
