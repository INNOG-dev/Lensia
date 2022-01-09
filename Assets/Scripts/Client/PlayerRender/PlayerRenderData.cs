using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerRenderData
{

    public uint skinId;

    public string skinColorsBase64;

    public PlayerRenderData()
    {

    }

    public Color[] getSkinColors()
    {
        if(skinColorsBase64 == null)
        {
            return null;
        }

        return NetworkUtils.byteArrayToColors(System.Convert.FromBase64String(skinColorsBase64));
    }

}
