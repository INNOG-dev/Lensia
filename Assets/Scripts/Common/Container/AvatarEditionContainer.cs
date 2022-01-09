using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MySql.Data.MySqlClient;
using System;

public class AvatarEditionContainer : Container
{

    public NetworkUser user;


    public AvatarEditionContainer(uint containerId,NetworkUser user) : base(containerId)
    {
        this.user = user;
    }

   
    public override List<ContainerSlot> nextElements()
    {
        List<ContainerSlot> slots = new List<ContainerSlot>();
        Server server = Server.getServer();

        MysqlHandler mysqlHandler = server.getMysqlHandler();

        if (!mysqlHandler.isConnected()) mysqlHandler.openConnection();

        MySqlCommand cmd = new MySqlCommand("SELECT skin_id FROM users_skins WHERE UID = @UID LIMIT @index,@elementCount", mysqlHandler.getConnection());
        cmd.Parameters.AddWithValue("@UID", user.getUID());
        cmd.Parameters.AddWithValue("@index", getLastElementIndex());
        cmd.Parameters.AddWithValue("@elementCount", getElementsPerPage());

        lastItemTransmittedIndex += getElementsPerPage();

        MySqlDataReader reader = cmd.ExecuteReader();

        allElementsSynchronized = true;
        while (reader.Read())
        {
            SkinSlot slot = new SkinSlot();
            slot.setSkin(reader.GetUInt32("skin_id"));
            containerElements.Add(slot);
            slots.Add(slot);
            allElementsSynchronized = false;
        }

        reader.Close();

        return slots;
    }

    public override void closeContainer()
    {
        lastItemTransmittedIndex = 0;
        clearElements();
        base.closeContainer();
    }

    public override void openContainer()
    {
        base.openContainer();
    }

}
