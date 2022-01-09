using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

public class NMSG_Profil : CallbackNMSG
{

    /*
     * 0: sync confidentiality
     * 1: select skin + colors
     * 2: result
     * 3: sync colors
     */
    private byte action;

    private byte fieldIndex;

    private bool state;

    private uint index;

    private Color[] skinColors = new Color[255];

    private uint containerId;

    /*
     * 0: ok
     * 1: skin not owned
     */
    private byte result;

    public NMSG_Profil() : base(null)
    {

    }

    public static NMSG_Profil syncConfidentiality(int fieldIndex, bool state)
    {
        NMSG_Profil packet = new NMSG_Profil();
        packet.action = 0;
        packet.fieldIndex = (byte)fieldIndex;
        packet.state = state;
        return packet;
    }

    public static NMSG_Profil selectSkin(uint containerId, Color[] skinColors, uint slotIndex)
    {
        NMSG_Profil packet = new NMSG_Profil();
        packet.action = 1;
        packet.containerId = containerId;
        packet.skinColors = skinColors;
        packet.index = slotIndex;
        return packet;
    }

    public static NMSG_Profil updateColors(uint containerId, Color[] skinColors)
    {
        NMSG_Profil packet = new NMSG_Profil();
        packet.action = 3;
        packet.containerId = containerId;
        packet.skinColors = skinColors;
        return packet;
    }


    public static NMSG_Profil syncResult(byte result)
    {
        NMSG_Profil packet = new NMSG_Profil();
        packet.action = 2;
        packet.result = result;
        return packet;
    }

    public override void HandleClient(Client client) { }

    public override void HandleServer(Server server, int networkId)
    {
        NetworkUser user = server.users[networkId];
        if (action == 0)
        {

            typeof(ConfidentialityData).GetFields(BindingFlags.Instance | BindingFlags.NonPublic)[fieldIndex].SetValue(user.getConfidentialityData(), state);

            user.setDirty(true);
        }
        else if(action == 1)
        {
            Container container = user.getOpenContainer(containerId);

            if(container is AvatarEditionContainer)
            {
                bool haveSkin = false;
                if (container.getElements().Count > index)
                {
                    SkinSlot slot = (SkinSlot)container.getElements()[(int)index];

                    user.getAccountData().setEquippedSkin(user,slot.getSkinUI().getSkin().getId(),skinColors);

                    user.setDirty(true);
                    
                    haveSkin = true;
                }

                result = (byte) (haveSkin ? 0 : 1);
                server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
            }
        }
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        base.ReadFromStream(reader);
        action = reader.ReadByte();
        if(action == 0)
        {
            fieldIndex = reader.ReadByte();
            state = reader.ReadBoolean();
        }
        else if(action == 1)
        {
            index = reader.ReadUInt32();
            containerId = reader.ReadUInt32();
            skinColors = NetworkUtils.readColors(reader);
        }
        else if(action == 2)
        {
            result = reader.ReadByte();
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        base.WriteToStream(writer);
        writer.Write(action);
        if(action == 0)
        {
            writer.Write(fieldIndex);
            writer.Write(state);
        }
        else if (action == 1)
        {
            writer.Write(index);
            writer.Write(containerId);
            NetworkUtils.writeColors(writer, skinColors);
        }
        else if (action == 2)
        {
            writer.Write(result);
        }
    }

    public byte getResult()
    {
        return result;
    }
}
