using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkSide
{

    public enum Side { CLIENT, SERVER };

    public static Side networkSide;


    public static bool isRemote()
    {
        return networkSide == Side.CLIENT;
    }


}

