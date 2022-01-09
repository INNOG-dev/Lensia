using MySql.Data.MySqlClient;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocialContainer : Container
{

    private NetworkUser user;

    /*
     *  0: friends list
     *  1: add friends
     *  2: blocklist
     */
    private byte containerType;

    private string keyword = "";

    private List<ContainerSlot> elementsFromKeyword = new List<ContainerSlot>();

    public SocialContainer(uint containerId, NetworkUser user) : base(containerId)
    {
        this.user = user;
    }

    public override List<ContainerSlot> nextElements()
    {
        elementsFromKeyword.Clear();
        List<ContainerSlot> slots = new List<ContainerSlot>();
        Server server = Server.getServer();

        MysqlHandler mysqlHandler = server.getMysqlHandler();

        if (!mysqlHandler.isConnected()) mysqlHandler.openConnection();

        MySqlCommand cmd = null;
        if (getContainerType() == 1)
        {
            cmd = new MySqlCommand("SELECT users_friends.UID1, users_friends.UID2,users.UID,users.network_id,users.username FROM users JOIN users_friends WHERE users_friends.type = @type AND (users_friends.UID1 = @UID AND users_friends.UID2 = users.UID OR users_friends.UID2 = @UID AND users_friends.UID1 = users.UID) LIMIT @index,@elementCount", mysqlHandler.getConnection());
        }
        else
        {
            cmd = new MySqlCommand("SELECT users.UID,users.network_id,users.username FROM users JOIN users_friends WHERE users_friends.type = @type AND (users_friends.UID1 = @UID AND users_friends.UID2 = users.UID OR users_friends.UID2 = @UID AND users_friends.UID1 = users.UID) LIMIT @index,@elementCount", mysqlHandler.getConnection());
        }

        cmd.Parameters.AddWithValue("@type", containerType);
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@index", getLastElementIndex());
        cmd.Parameters.AddWithValue("@elementCount", getElementsPerPage());

        lastItemTransmittedIndex += getElementsPerPage();

        MySqlDataReader reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            int networkId = reader.GetInt32("network_id");
            bool isConnected = server.users.ContainsKey(networkId);
            string username = reader.GetString("username");
            ulong uid = reader.GetUInt64("UID");

            SocialSlot slot = new SocialSlot((uint)containerElements.Count,uid);

            if(server.users.ContainsKey(networkId))
            {
                NetworkUser user = server.users[networkId];
                if (user.getPlayer() != null)
                {
                    slot.setMapId(user.getPlayer().getCurrentWorld().getWorldId());
                }
            }

            slot.setIsConnected(isConnected);
            slot.setUsername(username);

            byte requestType = 0;
            if (getContainerType() == 1)
            {
                ulong senderUID = reader.GetUInt64("UID1");
                ulong receiverUID = reader.GetUInt64("UID2");
                if (senderUID == uid)
                {
                    requestType = 1;
                }
                else if(receiverUID == uid)
                {
                    requestType = 2;
                }

                slot.setRequestType(requestType);
            }

            containerElements.Add(slot);

            slots.Add(slot);
        }
        reader.Close();

        if (keyword != "")
        {
            elementsFromKeyword.Clear();
            cmd = new MySqlCommand("SELECT users.UID,users.username FROM users JOIN users_friends WHERE users.username LIKE @Keyword AND users.UID != @UID LIMIT 5", mysqlHandler.getConnection());
            cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
            cmd.Parameters.AddWithValue("@UID", user.getUID());

            reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                string username = reader.GetString("username");
                ulong uid = reader.GetUInt64("UID");
                SocialSlot slot = new SocialSlot((uint)elementsFromKeyword.Count, uid);
                slot.setUsername(username);
                if (elementsFromKeyword.Contains(slot))
                {
                    continue;
                }
                slot.setRequestType(3);
                elementsFromKeyword.Add(slot);
                slots.Add(slot);
            }
            reader.Close();

            List<ContainerSlot> toRemove = new List<ContainerSlot>();
            foreach (SocialSlot slot in elementsFromKeyword)
            {
                if (user.getSocialManager().getRelationWith(slot.getUserUID()) != -1)
                {
                    toRemove.Add(slot);
                }
            }

            toRemove.ForEach(x =>
            {
                elementsFromKeyword.Remove(x);
                slots.Remove(x);
            });
            keyword = "";
        }

        return slots;
    }

    public override void closeContainer()
    {
        lastItemTransmittedIndex = 0;
        containerType = 0;
        clearElements();
        base.closeContainer();
    }

    public byte getContainerType()
    {
        return containerType;
    }

    public void setKeyword(string keyword)
    {
        this.keyword = keyword;
    }

    public List<ContainerSlot> getElementsFromKeyword()
    {
        return elementsFromKeyword;
    }

    public void setContainerType(byte type)
    {
        if (NetworkSide.isRemote())
        {
            Client client = Client.getClient();

            NMSG_Social packet = new NMSG_Social(7, type);
            packet.setCallback(delegate
            {
                closeContainer();
                containerType = type;
                openContainer();
            });

            client.processCallbackPacket(packet);
        }
        else
        {
            closeContainer();

            containerType = type;
            Debug.Log(containerType);

            openContainer();
        }
    }

    public override void openContainer()
    {
        base.openContainer();
    }

}
