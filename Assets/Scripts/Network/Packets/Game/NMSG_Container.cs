using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_Container : NMSG
{
    /*
     * 0: open container
     * 1: close container
     * 2: ask/send element
     */
    private byte action;

    private uint containerId;

    private List<ContainerSlot> elements = new List<ContainerSlot>();

    public NMSG_Container()
    {

    }
    public NMSG_Container(byte action, uint containerId)
    {
        this.action = action;
        this.containerId = containerId;
    }

    public NMSG_Container(List<ContainerSlot> elements, uint containerId)
    {
        this.action = 2;
        this.containerId = containerId;
        processElements(elements);
    }

    public override void HandleClient(Client client)
    {
        if(action == 2)
        {
            Container container = client.GetNetworkUser().getOpenContainer(containerId);

            if(container != null)
            {
                if (elements.Count == 0)
                {
                    container.setAllElementsSynchronized(true);
                }
                else
                {
                    foreach (ContainerSlot element in elements)
                    {
                        container.addElement(element);
                    }
                }
            }
        }
    }

    public override void HandleServer(Server server, int networkId)
    {
        NetworkUser user = server.users[networkId];
        if (action == 2)
        {
            Container container = user.getOpenContainer(containerId);
            if (container != null)
            {
                processElements(container.nextElements());
                    
                server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
            }
            else
            {
                Debug.LogWarning("User with networkid: " + networkId + " tried to interract with inexistant container");
            }
        }
        else if(action == 1)
        {
            user.closeContainer(containerId);
        }
        else if(action == 0)
        {
            user.openContainer(user.getContainerFromId(containerId));
        }
    }

    private void processElements(List<ContainerSlot> elements)
    {
        this.elements = elements;
    }
        
    public override void ReadFromStream(BinaryReader reader)
    {
        action = reader.ReadByte();
        containerId = reader.ReadUInt32();
        
        if (action == 2)
        {
            if (NetworkSide.isRemote())
            {
                int count = reader.ReadInt32();
                if(count > 0)
                {
                    string typeStr = reader.ReadString();
                    Type type = Type.GetType(typeStr);
                    for (int i = 0; i < count; i++)
                    {
                        ContainerSlot slot = (ContainerSlot)Activator.CreateInstance(type);
                        slot.ReadFromStream(reader);
                        elements.Add(slot);
                    }
                }
            }
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        writer.Write(action);
        writer.Write(containerId);
        if(action == 2)
        {
            if(!NetworkSide.isRemote())
            {
                writer.Write(elements.Count);

                if(elements.Count > 0)
                {
                    writer.Write(elements[0].GetType().Name);
                    foreach (ContainerSlot element in elements)
                    {
                        element.WriteToStream(writer);
                    }
                }
            }
        }
    }
}
