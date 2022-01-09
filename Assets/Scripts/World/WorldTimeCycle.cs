using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WorldTimeCycle : MonoBehaviour 
{
    [SerializeField]
    public enum TimeCycle
    {
        DAY,
        DUSK,
        NIGHT,
        DAWN
    };

    [SerializeField]
    public TimeCycle timeCylce;

    [SerializeField]
    public int fromHour;

    [SerializeField]
    public int toHour;

    [SerializeField]
    public GameObject mapParent;

    public float globalLightIntensity;
    public Color globalLightColor;




    public WorldTimeCycle()
    {

    }

    public bool updateTimeCycle(DateTime dateTime)
    {
        return DateUtils.isTimeBetween(dateTime, new TimeSpan(fromHour,0,0), new TimeSpan(toHour,0,0));
    }


}
