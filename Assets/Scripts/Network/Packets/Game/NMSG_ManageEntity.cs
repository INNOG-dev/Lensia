using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using UnityEngine;
using UnityEngine.Networking;

public class NMSG_ManageEntity : NMSG
{
    /*
     *  0 spawn/change world entities
     *  1 entity left world
     *  2 prevents all clients that entity change world
     */
    private byte action;

    private uint worldId;

    public bool isLocalPlayer = false;

    private List<EntityData> entities = new List<EntityData>();
    public NMSG_ManageEntity() { }

    public NMSG_ManageEntity(byte action, uint worldId)
    {
        this.action = action;
        this.worldId = worldId;
    }

    public NMSG_ManageEntity(byte action, EntityData data, uint worldId) 
    {
        this.action = action;
        this.worldId = worldId;
        appendEntity(data);
    }

    public NMSG_ManageEntity(byte action, EntityData data)
    {
        this.action = action;
        appendEntity(data);
    }

    public void appendEntity(EntityData data)
    {
        entities.Add(data);
    }

    public void setWorldId(uint worldId)
    {
        this.worldId = worldId;
    }

    public override void HandleClient(Client client) 
    {
        if(action == 0)
        {
            client.StartCoroutine(spawnEntities(client));   
        }
        else if(action == 2)
        {
            foreach (EntityData data in entities)
            {
                if(!client.getWorldManager().entityExist(data.entityId))
                {
                    continue;
                }

                Entity entity = client.getWorldManager().getEntity(data.entityId);


                client.getWorldManager().destroyEntity(entity);
            }
        }
    }

    public override void HandleServer(Server server, int networkId)  { }

    public override void ReadFromStream(BinaryReader reader)
    {
        action = reader.ReadByte();

        int count = reader.ReadInt32();

        if (action == 0)
        {
            isLocalPlayer = reader.ReadBoolean();
            for (int i = 0; i < count; i++)
            {
                byte entityType = reader.ReadByte();
                if(entityType == 0)
                {
                    PlayerData data = new PlayerData();
                    data.entityType = 0;
                    data.entityId = reader.ReadUInt32();
                    data.position = NetworkUtils.readVector2(reader);
                    data.tagName = reader.ReadString();
                    data.updateRendering = reader.ReadBoolean();
                    entities.Add(data);
                }
                else
                {
                    EntityData data = new EntityData();
                    data.entityType = entityType;
                    data.entityId = reader.ReadUInt32();
                    data.position = NetworkUtils.readVector2(reader);
                    entities.Add(data);
                }
            }

            worldId = reader.ReadUInt32();
        }
        else if(action == 2)
        {
            for (int i = 0; i < count; i++)
            {
                EntityData data = new EntityData();

                data.entityId = reader.ReadUInt32();

                entities.Add(data);
            }

            worldId = reader.ReadUInt32();
        }

    }

    public override void WriteToStream(BinaryWriter writer) 
    {
        writer.Write(action);

        writer.Write(entities.Count);

        if(action == 0)
        {
            writer.Write(isLocalPlayer);
            for (int i = 0; i < entities.Count; i++)
            {
                EntityData data = entities[i];

                writer.Write(data.entityType);
                writer.Write(data.entityId);
                NetworkUtils.writeVector2(writer, data.position);

                if(data.entityType == 0)
                {
                    PlayerData pData = (PlayerData)data;
                    writer.Write(pData.tagName);
                    writer.Write(pData.updateRendering);
                }
            }

            writer.Write(worldId);
        }
        else if(action == 2)
        {
            for (int i = 0; i < entities.Count; i++)
            {
                EntityData data = entities[i];

                writer.Write(data.entityId);
            }

            writer.Write(worldId);
        }
        else
        {
            for (int i = 0; i < entities.Count; i++)
            {
                EntityData data = entities[i];

                writer.Write(data.entityId);
            }
        }
    }

    public class EntityData : INetworkSerializable
    {
        public byte entityType;

        public uint entityId;

        public Vector2 position;

        public EntityData()
        {

        }

        public EntityData(uint entityId)
        {
            this.entityId = entityId;
        }

        public EntityData(uint entityId, byte entityType, Vector2 position) //spawn entity
        {
            this.entityId = entityId;
            this.entityType = entityType;
            this.position = position;
        }

        public void ReadFromStream(BinaryReader reader) { }

        public void WriteToStream(BinaryWriter writer) { }
    }

    public class PlayerData : EntityData
    {
        public string tagName;

        public bool updateRendering = false;

        public PlayerData() { }


        public PlayerData(uint entityId, byte entityType, Vector2 position, string tagName, bool updateRendering) : base(entityId, entityType, position)
        {
            this.tagName = tagName;
            this.updateRendering = updateRendering;
        }

    }

    public IEnumerator spawnEntities(Client client)
    {
        foreach(EntityData entityData in entities)
        {
            if (isLocalPlayer)
            {
                PlayerData playerData = (PlayerData)entities[0];
                if (client.getWorldManager().entityExist(playerData.entityId))
                {

                    client.getWorldManager().moveEntityInWorld(worldId, client.getWorldManager().getEntity(playerData.entityId));
                }
                else
                {
                    Debug.Log("local player creation");

                    PlayerRenderData renderData = client.getRenderingManager().getRenderingData(playerData.tagName);
                    if (renderData != null && !playerData.updateRendering)
                    {
                        client.getWorldManager().createPlayer(renderData.skinId, playerData.tagName, playerData.entityId, worldId, playerData.position, renderData.getSkinColors(), true);
                    }
                    else
                    {
                        WWWForm form = new WWWForm();

                        form.AddField("username", playerData.tagName);

                        using (UnityWebRequest www = UnityWebRequest.Post(Client.apiUrl + "/getSkinData.php", form))
                        {
                            yield return www.SendWebRequest();

                            if (www.responseCode == 404)
                            {
                                Debug.Log(www.downloadHandler.text);
                            }
                            else
                            {
                                PlayerRenderData playerRenderData = JsonUtility.FromJson<PlayerRenderData>(www.downloadHandler.text);

                                client.getRenderingManager().pushData(playerData.tagName, playerRenderData);
    
                                client.getWorldManager().createPlayer(playerRenderData.skinId, playerData.tagName, playerData.entityId, worldId, playerData.position, playerRenderData.getSkinColors(), true);
                            }
                        }
                    }

                }
            }
            else
            {
                foreach (EntityData data in entities)
                {
                    if (data is PlayerData)
                    {
                        PlayerData playerData = (PlayerData)data;

                        PlayerRenderData renderData = client.getRenderingManager().getRenderingData(playerData.tagName);
                        if(renderData != null && !playerData.updateRendering)
                        {
                            client.getWorldManager().createPlayer(renderData.skinId, playerData.tagName, playerData.entityId, worldId, playerData.position, renderData.getSkinColors(), false);
                        }
                        else
                        {
                            WWWForm form = new WWWForm();
                            form.AddField("username", playerData.tagName);

                            using (UnityWebRequest www = UnityWebRequest.Post(Client.apiUrl + "/getSkinData.php", form))
                            {
                                yield return www.SendWebRequest();

                                if (www.responseCode == 404)
                                {
                                    Debug.Log(www.downloadHandler.text);
                                }
                                else
                                {
                                    PlayerRenderData playerRenderData = JsonUtility.FromJson<PlayerRenderData>(www.downloadHandler.text);

                                    client.getRenderingManager().pushData(playerData.tagName, playerRenderData);

                                    client.getWorldManager().createPlayer(playerRenderData.skinId, playerData.tagName, playerData.entityId, worldId, playerData.position, playerRenderData.getSkinColors(), false);
                                }
                            }
                        }
                    }
                    else
                    {
                        EntityType entityType = EntityRegistry.getEntityType(data.entityType);
                        client.getWorldManager().spawnEntity(entityType, data.entityId, worldId, data.position);
                    }
                }
            }
        }
    }
}
