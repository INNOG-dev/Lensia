using System.Collections.Generic;
using UnityEngine;

public class CachedEntities
{
    private Dictionary<uint, List<GameObject>> cachedPlayersSkins = new Dictionary<uint, List<GameObject>>();
    private Dictionary<uint, List<GameObject>> cachedEntities = new Dictionary<uint, List<GameObject>>();

    private Transform cachedObjectsTransform;

    public CachedEntities()
    {
        cachedObjectsTransform = GameObject.Find("CachedEntities").transform;
    }

    public GameObject loadEntityFromCache(Entity entity)
    {
        EntityType type = EntityRegistry.getEntityType(entity.GetType());
        if (isAvaibleInCache(type))
        {
            GameObject obj = cachedEntities[type.getId()][0];
            cachedEntities[type.getId()].RemoveAt(0);

            entity.defineFreeEntityGameObject(delegate
            {
                freeGameObject(obj, type);
            });
            return obj;
        }
        else
        {
            if (!cachedEntities.ContainsKey(type.getId())) cachedEntities.Add(type.getId(), new List<GameObject>());
            GameObject obj = Object.Instantiate(type.getEntityGameObject(), cachedObjectsTransform);
            entity.defineFreeEntityGameObject(delegate
            {
                freeGameObject(obj, type);
            });
            return obj;
        }
    }

    public GameObject loadPlayerFromCache(Player player, uint skinId)
    {
        if(isAvaibleInCache(skinId))
        {
            GameObject obj = cachedPlayersSkins[skinId][0];
            cachedPlayersSkins[skinId].RemoveAt(0);

            player.defineFreeEntityGameObject(delegate
            {
                freeGameObject(obj, skinId);
            });

            return obj;
        }
        else
        {
            if(!cachedPlayersSkins.ContainsKey(skinId)) cachedPlayersSkins.Add(skinId, new List<GameObject>());
            PlayerSkin playerSkin = RegistryManager.skinRegistry.get(skinId);
            GameObject obj = Object.Instantiate(playerSkin.getSkinGameObject(),cachedObjectsTransform);

            player.defineFreeEntityGameObject(delegate
            {
                freeGameObject(obj, skinId);
            });

            return obj;
        }
    }

    public bool isAvaibleInCache(EntityType entityType)
    {
        if (!cachedEntities.ContainsKey(entityType.getId()))
        {
            return false;
        }

        List<GameObject> list = cachedEntities[entityType.getId()];
        if (list == null || list.Count == 0) return false;

        return true;
    }


    public bool isAvaibleInCache(uint skinId)
    {
        if(!cachedPlayersSkins.ContainsKey(skinId))
        {
            return false;
        }

        List<GameObject> list = cachedPlayersSkins[skinId];
        if (list == null || list.Count == 0) return false;

        return true;
    }

    public void freeGameObject(GameObject obj, uint skinId)
    {
        obj.transform.SetParent(cachedObjectsTransform);

        List<GameObject> list = cachedPlayersSkins[skinId];
        if(list == null)
        {
            list = new List<GameObject>();
        }
        list.Add(obj);
    }

    public void freeGameObject(GameObject obj, EntityType type)
    {
        obj.transform.SetParent(cachedObjectsTransform);

        List<GameObject> list = cachedEntities[type.getId()];
        if (list == null)
        {
            list = new List<GameObject>();
        }
        list.Add(obj);
    }




}
