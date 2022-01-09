using System.IO;

public abstract class NMSG : INetworkSerializable
{

    public byte packetId;

    public abstract void HandleClient(Client client);

    public abstract void HandleServer(Server server, int networkId);

    public abstract void ReadFromStream(BinaryReader reader);

    public abstract void WriteToStream(BinaryWriter writer);
}
