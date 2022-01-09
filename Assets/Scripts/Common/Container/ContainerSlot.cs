using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ContainerSlot : INetworkSerializable
{

    public ContainerSlot()
    {

    }

    public virtual int getSize()
    {
        return 0;
    }

    public virtual void ReadFromStream(BinaryReader reader)
    {

    }

    public virtual void WriteToStream(BinaryWriter writer)
    {

    }
}
