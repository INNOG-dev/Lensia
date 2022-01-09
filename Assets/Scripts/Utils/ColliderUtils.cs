using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColliderUtils
{

    public static float getMinimumAltitude(Vector2[] points)
    {
        float minimumAltitude = points[0].y;
        for(int i = 1; i < points.Length; i++)
        {
            if(points[i].y <= minimumAltitude)
            {
                minimumAltitude = points[i].y;
            }
        }

        return minimumAltitude;
    }

    public static float getMaximumAltitude(Vector2[] points)
    {
        float maximumAltitude = points[0].y;
        for (int i = 1; i < points.Length; i++)
        {
            if (points[i].y >= maximumAltitude)
            {
                maximumAltitude = points[i].y;
            }
        }

        return maximumAltitude;
    }

}
