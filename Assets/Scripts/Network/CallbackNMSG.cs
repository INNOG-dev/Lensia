using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CallbackNMSG : NMSG
{

    public CallbackNMSG(Action callback)
    {
        packetUID = DateUtils.currentTimeMillis();
    }

    public bool packetReceived = false;

    public Action callback;

    public double packetUID;

    protected byte result;

    public void setCallback(Action action)
    {
        this.callback = action;
    }

    public byte getResult()
    {
        return result;
    }


    public override void HandleClient(Client client) { }

    public override void HandleServer(Server server, int networkId) { }

    public override void ReadFromStream(BinaryReader reader) { }

    public override void WriteToStream(BinaryWriter writer) 
    {
        writer.Write(packetUID);
    }

}
