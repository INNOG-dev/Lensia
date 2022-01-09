using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : RegistryObject
{
    private GameObject mapObject;

    public Map(string mapName)
    {
        setRegistryName(mapName);
        setUnlocalizedName(mapName);
    }

    public override void onRegistered()
    {
        mapObject = Resources.Load<GameObject>("GameResources/Maps/" + getId() + "/Maps");
    }

    public GameObject getGraphic()
    {
        return mapObject;
    }

}
