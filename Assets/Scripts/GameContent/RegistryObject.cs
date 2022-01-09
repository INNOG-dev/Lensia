using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class RegistryObject 
{

    private string unlocalizedName;

    private string registryName;

    private uint registryId;

    public uint getId()
    {
        return registryId;
    }

    public void setRegistryId(uint id)
    {
        if (Main.INSTANCE.gameIsInitialized())
        {
            throw new System.Exception("Game already initialized");
        }
        registryId = id;
    }

    public RegistryObject setRegistryName(string registryName)
    {
        this.registryName = registryName;
        return this;
    }

    public virtual void onRegistered()
    {

    }

    public RegistryObject setUnlocalizedName(string unlocalizedName)
    {
        this.unlocalizedName = unlocalizedName;
        return this;
    }

    public string getRegistryName()
    {
        return this.registryName;
    }

    protected string getUnlocalizedName()
    {
        return this.registryName;
    }




}
