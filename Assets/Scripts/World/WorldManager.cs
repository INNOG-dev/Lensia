using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldManager 
{
    private static uint entityIdCounter;

    private Dictionary<uint, World> loadedWorlds = new Dictionary<uint, World>(); //si le monde est chargé c'est qu'il y a au moins un joueur dedans

    private Dictionary<uint, Entity> entities = new Dictionary<uint, Entity>();

    public static CachedEntities cachedEntities;

    public WorldManager()
    {
        if(cachedEntities == null) cachedEntities = new CachedEntities();
    }

    public int getEntityCount()
    {
        return entities.Count;
    }
        

    public void clearEntities()
    {
        if(NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            entities.Clear();
            entities.Add(client.GetNetworkUser().getPlayer().getEntityId(), client.GetNetworkUser().getPlayer());
        }
    }

    public bool worldLoaded(uint worldId)
    {
        return loadedWorlds.ContainsKey(worldId);
    }

    public Dictionary<uint,World>.ValueCollection getLoadedWorlds()
    {
        return loadedWorlds.Values;
    }

    public bool entityExist(uint entityId)
    {
        return entities.ContainsKey(entityId);
    }

    public Entity getEntity(uint entityId)
    {
        return entities[entityId];
    }

    private World loadWorld(uint worldId)
    {
        Map map = RegistryManager.mapRegistry.get(worldId);

        World world = new World(Object.Instantiate(map.getGraphic()),map);
        world.getWorldObjectInstance().name = "Maps-" + worldId;

        loadedWorlds.Add(map.getId(), world);

        return world;
    }

    public void unloadWorld(uint worldId)
    {
        if (!worldLoaded(worldId)) return;

        World world = get(worldId);

        Object.Destroy(world.getWorldObjectInstance());

        loadedWorlds.Remove(worldId);
    }

    public World get(uint worldId)
    {
        World world = null;
        if(!worldLoaded(worldId))
        {
            world = loadWorld(worldId);
        }

        world = loadedWorlds[worldId];

        return world;
    }

    public int loadedWorldsCount()
    {
        return loadedWorlds.Count;
    }

    public uint nextEntityId()
    {
        uint nextId = entityIdCounter;

        ++entityIdCounter;

 
        while(entities.ContainsKey(nextId))
        {
            nextId = (uint)Random.Range(0, int.MaxValue);
        }

        return nextId;
    }

    //server side version
    public void createPlayer(int networkId, uint skinId, string username, Color[] skinColors)
    {
        if(!NetworkSide.isRemote())
        {
            Server server = Server.getServer();
            Player player = new Player(nextEntityId(), server.users[networkId]);

            entities.Add(player.getEntityId(), player);

            instantiatePlayer(skinId,username,player);
 
            player.loadData(server.getMysqlHandler());

            if(skinColors != null)
            {
                player.getSkinComposition().applyColors(skinColors);
                player.setColors(skinColors);
            }

            if (player.isFirstJoin())
            {

                moveEntityInWorld(ServerSettings.spawnMapId, player, ServerSettings.spawnCoordinates);

                player.sendMessage("<color=red>Bienvenue " + player.getName() + " sur le serveur");
                player.setFirstJoin(false);
            }
            else
            {

                player.sendMessage("<color=red>Rebonjour " + player.getName());

                moveEntityInWorld(player.getCurrentWorldId(), player, player.position);
            }

        }
        else
        {
            throw new UnityException("Method not useable in client side");
        }

    }

    //client side version
    public Player createPlayer(uint skinId, string username, uint entityId, uint spawnMapId, Vector2 spawnCoordinates, Color[] skinColors, bool isLocalPlayer)
    {
        if (NetworkSide.isRemote())
        {
            Client client = Client.getClient();
            Player player = new Player(entityId);

            if(isLocalPlayer)
            {
                client.GetNetworkUser().setPlayer(player);
            }

            entities.Add(player.getEntityId(), player);

            instantiatePlayer(skinId, username, player);

            if (skinColors != null)
            {
                player.getSkinComposition().applyColors(skinColors);
                player.setColors(skinColors);
            }

            client.getWorldManager().moveEntityInWorld(spawnMapId, player, spawnCoordinates);
            return player;
        }
        else
        {
            throw new UnityException("You cannot use this methode in server side");
        }
    }

    public void spawnEntity(EntityType entityType, uint spawnMapId, Vector2 spawnCoordinates)
    {
        if(!NetworkSide.isRemote())
        {
            spawnEntity(entityType, nextEntityId(), spawnMapId, spawnCoordinates);
        }
    }
    
    public void spawnEntity(EntityType entityType, uint entityId, uint spawnMapId, Vector2 spawnCoordinates)
    {
        if (entityType == null) return;

        Entity entity = (Entity)ReflectionHelper.instantiateDynamically(entityType.getTypeOfEntity(), new object[] { entityId });

        instantiateEntity(entity);

        entities.Add(entity.getEntityId(), entity);
        if (NetworkSide.isRemote())
        {
            Client.getClient().getWorldManager().moveEntityInWorld(spawnMapId, entity, spawnCoordinates);
        }
        else
        {
            Server.getServer().getWorldManager().moveEntityInWorld(spawnMapId, entity, spawnCoordinates);
        }
    }

    private static GameObject instantiateEntity(Entity entity)
    {
        GameObject entityObject = cachedEntities.loadEntityFromCache(entity);
        
        entity.initEntity(entityObject);
        
        return entityObject;
    }

    private static GameObject instantiatePlayer(uint skinId,string username, Player player)
    {
        GameObject entityObject = cachedEntities.loadPlayerFromCache(player,skinId);

        player.initEntity(entityObject, username);

        return entityObject;
    }

    public void moveEntityInWorld(uint mapId, Entity entity, Vector2? moveInCoordinate)
    {
        World world = get(mapId);

        if (entity.getCurrentWorld() != null) //if entity already in a world unload previous world and remove entity from world;
        {
            if(entity is Player)
            {
                if (entity.getCurrentWorld().isRemote())
                {
                    Player player = (Player)entity;
                    if(player.isLocalPlayer()) //only if local client player
                    {
                        entity.getCurrentWorld().onWorldUnload();

                        foreach(Entity worldEntity in entity.getCurrentWorld().getEntitiesInWorld().Values.ToList())
                        {
                            if(worldEntity != player) worldEntity.onEntityLeftWorld();
                            worldEntity.getCurrentWorld().removeEntityFromWorld(worldEntity);
                        }
                        clearEntities();
                    }
                }
            }

            entity.getCurrentWorld().removeEntityFromWorld(entity);
        }

        world.moveEntity(entity, moveInCoordinate);

        foreach (uint adjacentMapId in world.getWorldProperties().getAdjacentsMaps())
        {
            if (worldLoaded(adjacentMapId)) continue;

            loadWorld(adjacentMapId);
        }
    }

    public void destroyEntity(Entity entity)
    {
        entity.onEntityLeftWorld();

        entity.getCurrentWorld().removeEntityFromWorld(entity);

        entities.Remove(entity.getEntityId());

        if(!entity.getCurrentWorld().isRemote())
        {
            Server server = Server.getServer();

            NMSG_ManageEntity packet = new NMSG_ManageEntity(1, new NMSG_ManageEntity.EntityData(entity.getEntityId()));

            server.broadcastWorld(packet, entity.getCurrentWorld().getWorldId(), server.reliableChannel, server.gameChannel.getChannelId());
        }
    }

    public void moveEntityInWorld(uint mapId, Entity entity)
    {
        moveEntityInWorld(mapId, entity, null);
    }

}
