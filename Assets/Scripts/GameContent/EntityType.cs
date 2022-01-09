using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityType : RegistryObject
{

    private GameObject entityObject;

    private Type typeOfEntity;


    public EntityType(string registryName)
    {
        setRegistryName(registryName.ToLower());

        setUnlocalizedName(registryName);
    }

    public override void onRegistered()
    {
        if(getRegistryName() != "player")
        {
            string path = "GameResources/Entities/" + getId() + "/Entity";

            entityObject = Resources.Load<GameObject>(path);
        }

        base.onRegistered();
    }

    public GameObject getEntityGameObject()
    {
        return entityObject;
    }

    public void setType(Type type)
    {
        typeOfEntity = type;
    }

    public Type getTypeOfEntity()
    {
        return typeOfEntity;
    }



}
