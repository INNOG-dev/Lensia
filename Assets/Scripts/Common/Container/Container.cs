using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Container : INetworkPageTransmission<ContainerSlot>
{

    protected int lastItemTransmittedIndex;

    protected List<ContainerSlot> containerElements = new List<ContainerSlot>();

    private uint containerId;

    protected bool allElementsSynchronized;

    public Container(uint containerId)
    {
        this.containerId = containerId;
    }

    public uint getContainerId()
    {
        return containerId;
    }

    public List<ContainerSlot> getElements()
    {
        return containerElements;
    }

    public void clearElements()
    {
        containerElements.Clear();
    }

    public virtual void closeContainer()
    {
        clearElements();
        allElementsSynchronized = false;
    }

    public virtual void openContainer()
    {

    }

    public void askElement()
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            client.sendToServer(new NMSG_Container(2, getContainerId()), client.reliableChannel, client.gameChannel.getChannelId());
        }
    }

    public bool hasNewValue()
    {
        return containerElements.Count > 0;
    }

    public void addElement(ContainerSlot element)
    {
        containerElements.Add(element);
    }

    public virtual int getElementsPerPage()
    {
        return 24;
    }

    public virtual List<ContainerSlot> nextElements()
    {
        return new List<ContainerSlot>();
    }

    public int getLastElementIndex()
    {
        return lastItemTransmittedIndex;
    }

    public void setAllElementsSynchronized(bool state)
    {
        allElementsSynchronized = state;
    }

    public bool getAllElementsAreSynchronized()
    {
        return allElementsSynchronized;
    }
}
