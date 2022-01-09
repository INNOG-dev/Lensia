using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class World
{
    private Dictionary<uint, Player> players = new Dictionary<uint, Player>();

    private Dictionary<uint, Entity> entities = new Dictionary<uint, Entity>();

    private GameObject worldObjectInstance;

    private bool isClientWorld;

    private Map theMap;

    private WorldProperties properties;

    public static Light2D globalLight;

    private Collider2D groundCollider;


    public World(GameObject worldObjectInstance, Map theMap)
    {
        this.worldObjectInstance = worldObjectInstance;

        isClientWorld = NetworkSide.isRemote();

        this.theMap = theMap;

        if (isClientWorld)
        {
            if (globalLight == null)
            {
                globalLight = GameObject.Find("Global Light 2D").GetComponent<Light2D>();
            }
        }

        Transform colidersTransform = worldObjectInstance.transform.GetChild(worldObjectInstance.transform.childCount - 1);

        properties = worldObjectInstance.GetComponent<WorldProperties>();
        properties.init(colidersTransform, this);
    }

    public WorldProperties getWorldProperties()
    {
        return properties;
    }


    private void syncWorldToPlayers()
    {
        if (players.Count == 0) return;

        Server server = Server.getServer();

        NMSG_SyncPlayerState packet = NMSG_SyncPlayerState.SyncState();

        foreach (Entity entity in getEntitiesInWorld().Values)
        {

            if (entity.getCurrentWorld() == null) continue;

            if (entity.entityState.isDirty())
            {
                packet.appendData(new NMSG_SyncPlayerState.StateData(entity.getEntityId(), entity.getEntityGameObject().transform.position, entity.getVelocity(),(byte)entity.getFacingDirection(),  entity.entityState));
                entity.entityState.sync();
            }
            else
            {
                packet.appendData(new NMSG_SyncPlayerState.StateData(entity.getEntityId(), entity.getEntityGameObject().transform.position, entity.getVelocity(), (byte)entity.getFacingDirection(), null));
            }

        }

        server.broadcastWorld(packet, getWorldId(), server.fragmentedUnreliableChannel, server.gameChannel.getChannelId());
       

    }

    private void syncWorldToSpawningPlayer(Player player)
    {
        if (!isRemote())
        {
            Server server = Server.getServer();

            NMSG_ManageEntity packetsSpawnEntitiesForPlayer = new NMSG_ManageEntity(0, theMap.getId());          

            NMSG_SyncPlayerState packetsSyncEntitiesStateForPlayer = NMSG_SyncPlayerState.SyncState();

            NMSG_ManageEntity packetSpawnPlayerToOthers;

            foreach (Entity otherEntity in entities.Values)
            {
                //sync all entities to current player
                if(otherEntity is Player)
                {
                    Player otherPlayer = (Player)otherEntity;
                    if(otherPlayer != player)
                    {
                        if(!player.getNetworkUser().getCachedClientData().synchedUserSkinData.Contains(otherPlayer.getNetworkUser().getUID()))
                        {
                            packetsSpawnEntitiesForPlayer.appendEntity(new NMSG_ManageEntity.PlayerData(otherPlayer.getEntityId(), 0, otherPlayer.getEntityGameObject().transform.position, otherPlayer.getName(), true));
                            player.getNetworkUser().getCachedClientData().markUserAsSynched(otherPlayer.getNetworkUser());
                        }
                        else
                        {
                            packetsSpawnEntitiesForPlayer.appendEntity(new NMSG_ManageEntity.PlayerData(otherPlayer.getEntityId(), 0, otherPlayer.getEntityGameObject().transform.position, otherPlayer.getName(), false));
                        }
                    }
                    else
                    {
                        NMSG_ManageEntity packetSpawnLocalPlayer = null;
                        if (!player.getNetworkUser().getCachedClientData().synchedUserSkinData.Contains(otherPlayer.getNetworkUser().getUID()))
                        {
                            packetSpawnLocalPlayer = new NMSG_ManageEntity(0, new NMSG_ManageEntity.PlayerData(player.getEntityId(), 0, player.getEntityGameObject().transform.position, player.getName(), true), getWorldId());
                            player.getNetworkUser().getCachedClientData().markUserAsSynched(otherPlayer.getNetworkUser());
                        }
                        else
                        {
                            packetSpawnLocalPlayer = new NMSG_ManageEntity(0, new NMSG_ManageEntity.PlayerData(player.getEntityId(), 0, player.getEntityGameObject().transform.position, player.getName(), false), getWorldId());
                        }

                        packetSpawnLocalPlayer.isLocalPlayer = true;
                        server.sendToClient(packetSpawnLocalPlayer, server.reliableChannel, server.gameChannel.getChannelId(), player.getNetworkUser().getNetworkId());
                    }
                }
                else
                {
                    packetsSpawnEntitiesForPlayer.appendEntity(new NMSG_ManageEntity.EntityData(otherEntity.getEntityId(), (byte)EntityRegistry.getEntityType(otherEntity.GetType()).getId(), otherEntity.getEntityGameObject().transform.position));
                }

                if (otherEntity != player)
                {
                   packetsSyncEntitiesStateForPlayer.appendData(new NMSG_SyncPlayerState.StateData(otherEntity.getEntityId(), otherEntity.getEntityGameObject().transform.position, otherEntity.getVelocity(), (byte)otherEntity.getFacingDirection(), otherEntity.entityState));
                }

                //sync to others player the current player
                if (otherEntity is Player && otherEntity.getEntityId() != player.getEntityId())
                {
                    Player otherPlayer = (Player)otherEntity;
                   
                    packetSpawnPlayerToOthers = new NMSG_ManageEntity(0, new NMSG_ManageEntity.PlayerData(player.getEntityId(),0, player.getEntityGameObject().transform.position, player.getName(), false), theMap.getId());
                    
                    server.sendToClient(packetSpawnPlayerToOthers, server.reliableChannel, server.gameChannel.getChannelId(), otherPlayer.getNetworkUser().getNetworkId());
                }
            }


           server.sendToClient(packetsSpawnEntitiesForPlayer, server.fragmentedReliableChannel, server.gameChannel.getChannelId(), player.getNetworkId()); 
           server.sendToClient(packetsSyncEntitiesStateForPlayer, server.fragmentedReliableChannel, server.gameChannel.getChannelId(), player.getNetworkId());
            

        }

    }

    public void syncSpawnEntityToPlayers(Entity entity)
    {
        if (!isRemote())
        {
            Server server = Server.getServer();

            NMSG_ManageEntity packetSpawnEntityForPlayers = new NMSG_ManageEntity(0, theMap.getId());
            packetSpawnEntityForPlayers.appendEntity(new NMSG_ManageEntity.EntityData(entity.getEntityId(), (byte)EntityRegistry.getEntityType(entity.GetType()).getId(), entity.getEntityGameObject().transform.position));

            server.broadcastWorld(packetSpawnEntityForPlayers, getWorldId(), server.reliableChannel, server.gameChannel.getChannelId());

        }
    }

    public void moveEntity(Entity entity, Vector2? coordinates)
    {
        if(entity is Player)
        {
            Player player = (Player)entity;
            players.Add(entity.getEntityId(), player);

            if (coordinates != null)
            {
                Vector2 castCoordinates = (Vector2)coordinates;
                player.serverCorrectMovement = true;
                player.lastPosition = castCoordinates;
                player.position = castCoordinates;

                entity.getEntityGameObject().transform.position = castCoordinates;
            }
        }
        else
        {
            if (coordinates != null)
            {
                Vector2 castCoordinates = (Vector2)coordinates;
                entity.lastPosition = castCoordinates;
                entity.position = castCoordinates;
                entity.getEntityGameObject().transform.position = castCoordinates;
            }
        }

        entities.Add(entity.getEntityId(), entity);

        if(entity is Player)
        {
            Player player = (Player)entity;

            syncWorldToSpawningPlayer(player);

            if (isRemote())
            {
                Client client = Client.getClient();
                if (player.isLocalPlayer())
                {
                    player.getController().getCameraController().placeCamera(new Vector3(worldObjectInstance.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z));

                    getWorldProperties().updateWorldTimeCycle();

                    foreach (uint adjacentMapId in getWorldProperties().getAdjacentsMaps())
                    {
                        if (client.getWorldManager().worldLoaded(adjacentMapId))
                        {
                            client.getWorldManager().get(adjacentMapId).getWorldProperties().disableTimeCycles();
                        }
                    }
                }
                onWorldLoad();
            }
        }
        else
        {
            syncSpawnEntityToPlayers(entity);
        }


        entity.setCurrentWorld(this);

        entity.setScale(entity.defaultScale * getWorldProperties().scaleModificator);

        //entityObject.transform.SetParent(worldObjectInstance.transform);
        //entityObject.SetActive(true);

        entity.getEntityGameObject().transform.SetParent(worldObjectInstance.transform);
        entity.getEntityGameObject().SetActive(true);

        entity.onEntitySpawned();
    }

    public void removeEntityFromWorld(Entity entity)
    {
        if (entities.ContainsKey(entity.getEntityId()))
        {
            if (!isRemote())
            {
                Server server = Server.getServer();

                NMSG_ManageEntity packet = new NMSG_ManageEntity(2, new NMSG_ManageEntity.EntityData(entity.getEntityId()), getWorldId());

                if(entity is Player)
                {
                    Player player = (Player)entity;
                    Debug.Log(getWorldId());
                    server.broadcastWorld(packet, getWorldId(), server.reliableChannel, server.gameChannel.getChannelId(),player.getNetworkId());
                }
                else
                {
                    server.broadcastWorld(packet, getWorldId(), server.reliableChannel, server.gameChannel.getChannelId());
                }
                
                //send packet to remove entity from world
            }

            if (entity is Player)
            {
                players.Remove(entity.getEntityId());
            }

            entities.Remove(entity.getEntityId());

        }
    }

    public void update()
    {
        foreach (Entity entity in getEntitiesInWorld().Values.ToList())
        {
            entity.update();
        }
    }

    public void fixedUpdate(uint tick)
    {
        foreach (Entity entity in getEntitiesInWorld().Values.ToList())
        {
            entity.fixedUpdate();

            if(entity is Player)
            {
                Player player = (Player)entity;
                if (!isRemote() && tick % (Server.fixedUpdateTicks * 5) == 0 && player.getIsDirty())
                {
                    Server server = Server.getServer();
                    player.saveData(server.getMysqlHandler());
                }
            }    
        }

        if (tick % (Server.fixedUpdateTicks * 60) == 0)
        {
            if (!isRemote())
            {
                Server server = Server.getServer();
                if (shouldUnloadWorld())
                {
                    server.getWorldManager().unloadWorld(getWorldId());
                }
            }
            else
            {
                Client client = Client.getClient();
                if (client.GetNetworkUser().getPlayer().getCurrentWorld() == this)
                {
                    getWorldProperties().updateWorldTimeCycle();
                }
                else
                {
                    getWorldProperties().disableTimeCycles();
                }
            }
        }

        if(!isRemote())
        {
            syncWorldToPlayers();
        }
    }

    public bool shouldUnloadWorld()
    {
        Server server = Server.getServer();
        if(entities.Count == 0)
        {
            foreach(uint adjacentMaps in getWorldProperties().getAdjacentsMaps())
            {
                if (!server.getWorldManager().worldLoaded(adjacentMaps)) continue;

                if(server.getWorldManager().get(adjacentMaps).getEntitiesInWorld().Count > 0)
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public uint getWorldId()
    {
        return theMap.getId();
    }

    public GameObject getWorldObjectInstance()
    {
        return worldObjectInstance;
    }

    public bool isRemote()
    {
        return isClientWorld;
    }

    public Dictionary<uint,Player> getPlayersInWorld()
    {
        return players;
    }

    public Dictionary<uint, Entity> getEntitiesInWorld()
    {
        return entities;
    }

    public void onWorldUnload()
    {
        AudioSource source = worldObjectInstance.GetComponent<AudioSource>();
        if (source != null) source.enabled = false;
    }

    public void onWorldLoad()
    {
        AudioSource source = worldObjectInstance.GetComponent<AudioSource>();
        if (source != null) source.enabled = true;
    }


}
