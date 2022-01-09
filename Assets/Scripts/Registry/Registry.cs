
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Registry<E> where E : RegistryObject
{
    private uint index;

    private Dictionary<uint, E> registered = new Dictionary<uint, E>(); 

    public void register(E e)
    {

        if(registered.ContainsValue(e))
        {
            throw new System.ArgumentException("Object already registered in registre");
        }

        e.setRegistryId(index);
        registered.Add(index,e);
        ++index;

        e.onRegistered();

        Debug.Log("Object Type " + e.GetType().Name + " registered with id : " + index);
    }

    public void registerAll(E[] es)
    {
        foreach(E e in es)
        {
            register(e);
        }

    }

    public E get(uint index)
    {
        if(!registered.ContainsKey(index))
        {
            throw new System.ArgumentException("Object not registered");
        }

        return registered[index];
    }

    public List<E> getValues()
    {
        return registered.Values.ToList();
    }



}
