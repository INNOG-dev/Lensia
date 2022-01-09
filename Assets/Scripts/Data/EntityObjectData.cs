using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityObjectData
{

    private GameObject gameObject;

    private int objectHash;

    public EntityObjectData(GameObject gameObject, int hashCode)
    {
        this.gameObject = gameObject;
        this.objectHash = hashCode;
    }

    public GameObject getGameObject()
    {
        return gameObject;
    }

    public override bool Equals(object obj)
    {
        if(obj is EntityObjectData)
        {
            EntityObjectData entityObjectData = (EntityObjectData)obj;
            return entityObjectData.objectHash == objectHash;
        }

        return false;
    }

}
