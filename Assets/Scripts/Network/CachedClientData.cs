using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CachedClientData 
{

    public List<long> synchedUserSkinData = new List<long>();


    public void markUserAsSynched(NetworkUser user)
    {
        synchedUserSkinData.Add(user.getUID());
    }

    public void removeUser(NetworkUser user)
    {
        synchedUserSkinData.Remove(user.getUID());
    }

}
