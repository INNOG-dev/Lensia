using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class EntityState : INetworkSerializable
{
    private bool shouldSync;

    private List<object> states = new List<object>();

    public void markDirty()
    {
        shouldSync = true;
    }

    public bool isDirty()
    {
        return shouldSync;
    }

    public void sync()
    {
        shouldSync = false;
    }

    public void registerState(object value)
    {
        if (!value.GetType().IsPrimitive && !(value is string && value is INetworkSerializable))
        {
            throw new ArgumentException("Only string, primitive or NetworkSerializable argument is allowed");
        }

        states.Add(value);
    }

    public void setState(int index, object value)
    {
        if (states[index].GetType() != value.GetType())
        {
            throw new AccessViolationException("Can't modify property registry type");
        }
        states[index] = value;
        markDirty();
    }

    public object getValueAt(int index)
    {
        return states[index];
    }

    public void WriteToStream(BinaryWriter writer)
    {
        foreach (object obj in states)
        {
            if (obj is byte)
            {
                writer.Write((byte)obj);
            }
            else if (obj is bool)
            {
                writer.Write((bool)obj);
            }
            else if (obj is char)
            {
                writer.Write((char)obj);
            }
            else if (obj is byte[])
            {
                byte[] bytes = (byte[])obj;
                writer.Write(bytes.Length);
                writer.Write(bytes);
            }
            else if (obj is char[])
            {
                char[] chars = (char[])obj;
                writer.Write(chars.Length);
                writer.Write(chars);
            }
            else if (obj is decimal)
            {
                writer.Write((decimal)obj);
            }
            else if (obj is double)
            {
                writer.Write((double)obj);
            }
            else if (obj is float)
            {
                writer.Write((float)obj);
            }
            else if (obj is int)
            {
                writer.Write((int)obj);
            }
            else if (obj is long)
            {
                writer.Write((long)obj);
            }
            else if (obj is sbyte)
            {
                writer.Write((sbyte)obj);
            }
            else if (obj is short)
            {
                writer.Write((short)obj);
            }
            else if (obj is string)
            {
                writer.Write((string)obj);
            }
            else if (obj is uint)
            {
                writer.Write((uint)obj);
            }
            else if (obj is ulong)
            {
                writer.Write((ulong)obj);
            }
            else if (obj is ushort)
            {
                writer.Write((ushort)obj);
            }
            else if (obj is INetworkSerializable)
            {
                INetworkSerializable serializable = (INetworkSerializable)obj;
                serializable.WriteToStream(writer);
            }
        }
    }

    public void ReadFromStream(BinaryReader reader)
    {
        int index = 0;
        foreach (object obj in states.ToList())
        {
            if (obj is byte)
            {
                states[index] = reader.ReadByte();
            }
            else if (obj is bool)
            {
                states[index] = reader.ReadBoolean();
            }
            else if (obj is char)
            {
                states[index] = reader.ReadChar();
            }
            else if (obj is byte[])
            {
                int count = reader.ReadInt32();
                states[index] = reader.ReadBytes(count);
            }
            else if (obj is char[])
            {
                int count = reader.ReadInt32();
                states[index] = reader.ReadChars(count);
            }
            else if (obj is decimal)
            {
                states[index] = reader.ReadDecimal();
            }
            else if (obj is double)
            {
                states[index] = reader.ReadDouble();
            }
            else if (obj is float)
            {
                states[index] = reader.ReadSingle();
            }
            else if (obj is int)
            {
                states[index] = reader.ReadInt32();
            }
            else if (obj is long)
            {
                states[index] = reader.ReadInt64();
            }
            else if (obj is sbyte)
            {
                states[index] = reader.ReadSByte();
            }
            else if (obj is short)
            {
                states[index] = reader.ReadInt16();
            }
            else if (obj is string)
            {
                states[index] = reader.ReadString();
            }
            else if (obj is uint)
            {
                states[index] = reader.ReadUInt32();
            }
            else if (obj is ulong)
            {
                states[index] = reader.ReadUInt64();
            }
            else if (obj is ushort)
            {
                states[index] = reader.ReadUInt16();
            }
            else if (obj is INetworkSerializable)
            {
                INetworkSerializable serializable = (INetworkSerializable)states[index];
                serializable.ReadFromStream(reader);
            }

            ++index;
        }
    }

    public int getStateCount()
    {
        return states.Count;
    }

}
