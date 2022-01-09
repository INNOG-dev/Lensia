using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public interface INetworkSerializable
{
    public abstract void WriteToStream(BinaryWriter writer);

    public abstract void ReadFromStream(BinaryReader reader);

}
