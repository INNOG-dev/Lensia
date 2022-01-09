using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPath
{
    
    public struct PathData
    {
        private Vector2 point;
        private float scaleModificator;

        public PathData(Vector2 point, float scaleModificator)
        {
            this.point = point;
            this.scaleModificator = scaleModificator;
        }

        public Vector2 getPoint() { return point; }

        public float getDistanceXWith(float posX)
        {
            return (posX - point.x) * (posX - point.x);
        }

        public float getScaleModificator() { return scaleModificator; }
    }

    private List<PathData> pathDatas = new List<PathData>();

    public float mapMinimumAltitude;

    public float mapMaximumAltitude;

    public float averrageAltitude;


    public WorldPath(EdgeCollider2D collider)
    {
        mapMinimumAltitude = ColliderUtils.getMinimumAltitude(collider.points);
        mapMaximumAltitude = ColliderUtils.getMaximumAltitude(collider.points);

        if((mapMaximumAltitude - mapMinimumAltitude) * (mapMaximumAltitude - mapMinimumAltitude) >= 5)
        {
            averrageAltitude = mapMaximumAltitude;
        }
        else
        {
            averrageAltitude = (mapMaximumAltitude + mapMinimumAltitude) / 2f;
        }



        constructPath(collider);
    }

    public List<PathData> getPathDatas()
    {
        return pathDatas;
    }

    private void constructPath(EdgeCollider2D collider)
    {
        for(int i = 0; i < collider.pointCount; i ++)
        {
            
            PathData pathData = new PathData(collider.points[i], getScaleFromCameraDistance(collider.points[i]));

            pathDatas.Add(pathData);
        }
    }

    private float getScaleFromCameraDistance(Vector2 pathPoint)
    {
        return Mathf.Abs(pathPoint.y / averrageAltitude);
    }

    public PathData getNearestPathDataToPlayer(Player p)
    {
        PathData nearestPath = pathDatas[0];
        float nearestDistanceX = nearestPath.getDistanceXWith(p.getEntityGameObject().transform.localPosition.x);
        

        for(int i = 1; i < pathDatas.Count; i++)
        {
            PathData currentPath = pathDatas[i];
            float pathDistance = currentPath.getDistanceXWith(p.getEntityGameObject().transform.localPosition.x);

            if (pathDistance < nearestDistanceX)
            {
                
                nearestPath = currentPath;
                nearestDistanceX = pathDistance;
            }
        }
        return nearestPath;
    }

}
