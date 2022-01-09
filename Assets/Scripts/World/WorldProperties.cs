using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorldProperties : MonoBehaviour
{

    private Dictionary<int,ColliderObject> tiles = new Dictionary<int, ColliderObject>();

    [SerializeField]
    private List<uint> adjacentMaps = new List<uint>();

    public Vector2 cameraMaxCoordinates;
    public Vector2 cameraMinCoordinates;

    public float scaleModificator = 1f;

    private WorldTimeCycle[] timeCycles;

    public WorldPath path;

    private World world;

    private WorldTimeCycle currentActiveTime;

    public void init(Transform collidersTransform, World world)
    {
        this.world = world;

        Transform obstaclesTransform = collidersTransform.GetChild(0);

        if (obstaclesTransform.childCount >= 1)
        {
            Collider2D groundCollider = obstaclesTransform.GetChild(0).GetComponent<Collider2D>();
            if (groundCollider != null)
            {
                if (groundCollider is EdgeCollider2D)
                {
                    path = new WorldPath((EdgeCollider2D)groundCollider);
                }
            }
        }

        Transform tilesTransform = collidersTransform.GetChild(1);
        foreach(Transform transform in tilesTransform)
        {
            ColliderObject tileCollider = transform.GetComponent<ColliderObject>();
            if(tileCollider != null)
            {
                tiles.Add(tileCollider.getCollider2D().GetInstanceID(), tileCollider);
            }
        }

        timeCycles = gameObject.GetComponents<WorldTimeCycle>();
    }

    public List<uint> getAdjacentsMaps()
    {
        return adjacentMaps;
    }

    public void updateWorldTimeCycle()
    {
        foreach (WorldTimeCycle timeCycle in timeCycles)
        {

            if (timeCycle.updateTimeCycle(System.TimeZoneInfo.ConvertTime(System.DateTime.Now, Main.INSTANCE.getTimezone())))
            {

                if (currentActiveTime != null)
                {
                    currentActiveTime.mapParent.SetActive(false);
                }
                else
                {
                    foreach (WorldTimeCycle timeCycle1 in timeCycles)
                    {
                        if(timeCycle1 != timeCycle)
                        {
                            timeCycle1.mapParent.SetActive(false);
                        }
                    }
                }

                World.globalLight.intensity = timeCycle.globalLightIntensity;
                World.globalLight.color = timeCycle.globalLightColor;
                currentActiveTime = timeCycle;

                currentActiveTime.mapParent.SetActive(true);
                return;
            }
        }
    }

    public void disableTimeCycles()
    {
        foreach (WorldTimeCycle timeCycle1 in timeCycles)
        {        
             timeCycle1.mapParent.SetActive(false);   
        }
    }

    public ColliderObject getColliderObject(int id)
    {
        if(tiles.ContainsKey(id))
        {
            return tiles[id];
        }
        return null;
    }



}
