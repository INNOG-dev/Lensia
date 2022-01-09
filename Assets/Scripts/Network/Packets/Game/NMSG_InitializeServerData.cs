using System.IO;
using UnityEngine;

public class NMSG_InitializeServerData : NMSG
{
    public int networkId;
    public string timezoneId;

    public NMSG_InitializeServerData()
    {
        
    }

    public NMSG_InitializeServerData(int networkId)
    {
        this.networkId = networkId;
        System.TimeZoneInfo info = System.TimeZoneInfo.Local;
        timezoneId = info.Id;
    }

    public override void HandleClient(Client client)
    {
        client.setLocalNetworkUser(networkId);
        Main.INSTANCE.setTimezoneId(timezoneId);
    }

    public override void HandleServer(Server server, int networkId)
    {
       
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        networkId = reader.ReadInt32();
        timezoneId = reader.ReadString();
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(networkId);
        writer.Write(timezoneId);
    }
}
