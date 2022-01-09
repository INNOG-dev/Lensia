using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_KickPlayer : NMSG
{
    private string message;

    public NMSG_KickPlayer()
    {

    }

    public NMSG_KickPlayer(string kickMessage)
    {
        this.message = kickMessage;
    }

    public override void HandleClient(Client client)
    {
        client.StopAllCoroutines();
        InformationsBox box = new InformationsBox(message, "Ok");
        box.displayDialogBox(Main.INSTANCE.getMainMenuUI().transform);
    }

    public override void HandleServer(Server server, int networkId) { }

    public override void ReadFromStream(BinaryReader reader)
    {
        message = reader.ReadString();
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(message);
    }
}
