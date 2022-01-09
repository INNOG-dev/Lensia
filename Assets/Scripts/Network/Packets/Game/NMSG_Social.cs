using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class NMSG_Social : CallbackNMSG
{

    /*
     *  0: add friend
     *  1: remove friend/cancel friend request
     *  2: accept friend request
     *  3: decline friend request
     *  4: block user
     *  5: unblock user
     *  6: send result to client
     *  7: set container type
     *  8: search friend by input
     */
    private byte action;

    private uint slotIndex;

    private byte containerType;

    private string keyword;

    public NMSG_Social() : base(null)
    {

    }

    public NMSG_Social(byte action, uint slotIndex) : base(null)
    {
        this.action = action;
        this.slotIndex = slotIndex;
    }

    public NMSG_Social(string keyword) : base(null)
    {
        this.action = 8;
        this.keyword = keyword;
    }

    public NMSG_Social(byte action, byte containerType) : base(null)
    {
        this.action = action;
        this.containerType = containerType;
    }

    public override void HandleClient(Client client) 
    {
        if(action == 8)
        {
            SocialContainer container = (SocialContainer)client.GetNetworkUser().getOpenContainer(1);
            container.setAllElementsSynchronized(false);
        }
    }

    public override void HandleServer(Server server, int networkId) 
    {
        NetworkUser user = server.users[networkId];
        SocialManager socialManager = user.getSocialManager();
        SocialContainer container = (SocialContainer)user.getOpenContainer(1);

        if (container != null)
        {
            if (action == 7)
            {
                container.setContainerType(containerType);

                action = 6;
                result = 0;
                server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
            }
            else if (action == 8)
            {
                container.setKeyword(keyword);
            }
            else if(action == 0)
            {
                SocialSlot slot = (SocialSlot)container.getElementsFromKeyword()[(int)slotIndex];
                result = socialManager.sendFriendRequest(slot.getUserUID());
                action = 6;
                server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
            }
            else
            {
                SocialSlot slot = (SocialSlot)container.getElements()[(int)slotIndex];
                if (slot != null)
                {
                    if (action == 1)
                    {
                        result = fromBool(socialManager.removeFriend(slot.getUserUID()));
                    }
                    else if (action == 2)
                    {
                        result = fromBool(socialManager.acceptFriendRequest(slot.getUserUID()));
                    }
                    else if (action == 3)
                    {
                        result = fromBool(socialManager.declineFriendRequest(slot.getUserUID()));
                    }
                    else if (action == 4)
                    {
                        result = fromBool(socialManager.blacklistUser(slot.getUserUID()));
                    }
                    else if (action == 5)
                    {
                        result = fromBool(socialManager.unBlacklistUser(slot.getUserUID()));
                    }

                    action = 6;
                    server.sendToClient(this, server.reliableChannel, server.gameChannel.getChannelId(), networkId);
                }
            }
        }
    }

    public int getSlotIndex()
    {
        return (int)slotIndex;
    }

    private byte fromBool(bool state)
    {
        if (state) return 1;
        
        return 0;
    }

    public override void ReadFromStream(BinaryReader reader)
    {
        base.ReadFromStream(reader);

        action = reader.ReadByte();

        if(action != 7 && action != 8) slotIndex = reader.ReadUInt32();

        if (action == 6)
        {
            result = reader.ReadByte();
        }
        else if (action == 7)
        {
            containerType = reader.ReadByte();
        }
        else if (action == 8)
        {
            keyword = reader.ReadString();
        }
    }

    public override void WriteToStream(BinaryWriter writer)
    {
        base.WriteToStream(writer);

        writer.Write(action);

        if(action != 7 && action != 8) writer.Write(slotIndex);

        if(action == 6)
        {
            writer.Write(result);
        }
        else if(action == 7)
        {
            writer.Write(containerType);
        }
        else if(action == 8)
        {
            writer.Write(keyword);
        }
    }

}
