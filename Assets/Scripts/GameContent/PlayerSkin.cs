using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSkin : RegistryObject
{
    private GameObject skinObject;

    public PlayerSkin(string registryName)
    {
        setRegistryName(registryName.ToLower());

        setUnlocalizedName(registryName);
    }
    
    public override void onRegistered()
    {
        string path = "GameResources/Skins/" + getId() + "/Player";
        skinObject = Resources.Load<GameObject>(path);

        base.onRegistered();
    }

    public GameObject getSkinGameObject()
    {
        return skinObject;
    }


}
