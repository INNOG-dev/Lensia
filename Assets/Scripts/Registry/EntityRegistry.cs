using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityRegistry
{
    private static Dictionary<Type, EntityType> entitiesRegistry = new Dictionary<Type, EntityType>();
    private static Dictionary<uint, EntityType> entitiesRegistryId = new Dictionary<uint, EntityType>();

    private static uint entitiesRegistryIdCounter;

    public static void registerEntity(Type type, EntityType entityType)
    {
        if(type.IsSubclassOf(typeof(Entity)))
        {
            if(entitiesRegistry.ContainsKey(type))
            {
                throw new Exception("Ce type d'entité est déjà enregistré");
            }

            entityType.setRegistryId((uint)entitiesRegistry.Count);
            entityType.setType(type);

            entitiesRegistry.Add(type, entityType);

            entityType.setRegistryId(entitiesRegistryIdCounter);
            entitiesRegistryId.Add(entitiesRegistryIdCounter, entityType);

            Debug.Log("Entity " + entityType.getRegistryName() + " registered!");

            entityType.onRegistered();
            ++entitiesRegistryIdCounter;
        }
    }

    public static EntityType getEntityType(Type type)
    {
        
        if (entitiesRegistry.ContainsKey(type))
        {
            return entitiesRegistry[type];
        }

        return null;
    }

    public static EntityType getEntityType(uint id)
    {

        if (entitiesRegistryId.ContainsKey(id))
        {
            return entitiesRegistryId[id];
        }

        return null;
    }

}
