using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ColliderHandler
{

    private List<ColliderObject> triggeredObjects = new List<ColliderObject>();

    public void FixedUpdate(Entity entity)
    {
        handleCollider(entity);
    }

    public MovementAffectColliderObject getMovementModificator()
    {
        return (MovementAffectColliderObject)triggeredObjects.FirstOrDefault(collider => collider is MovementAffectColliderObject);
    }

    public void handleCollider(Entity entity)
    {
        RaycastHit2D[] raycasts = Physics2D.RaycastAll(entity.getEntityGameObject().transform.position, entity.getVelocity(), 0f);

        List<ColliderObject> stillCollidedObjects = new List<ColliderObject>();
        foreach (RaycastHit2D raycast in raycasts)
        {
            if (!entity.getCurrentWorld().isRemote())
            {
                if (raycast.collider.gameObject.name.Contains("Maps"))
                {
                    uint mapId = uint.Parse(raycast.collider.gameObject.name.Split('-')[1]);

                    Server server = Server.getServer();

                    if (entity.getCurrentWorld().getWorldId() != mapId)
                    {
                        server.getWorldManager().moveEntityInWorld(mapId, entity);
                        return;
                    }
                }
            } 
            else
            {
                ColliderObject colliderObject = entity.getCurrentWorld().getWorldProperties().getColliderObject(raycast.collider.GetInstanceID());
                if (colliderObject != null)
                {
                    if (!colliderObject.isAlreadyTriggered(entity.getEntityGameObject()))
                    {
                        colliderObject.onTriggerEnter(entity.getEntityGameObject());
                        triggeredObjects.Add(colliderObject);
                    }

                    stillCollidedObjects.Add(colliderObject);
                }
            }
        }

        foreach (ColliderObject colliderObject in triggeredObjects.ToList())
        {
            if (!stillCollidedObjects.Contains(colliderObject))
            {
                colliderObject.onTriggerExit(entity.getEntityGameObject());
                triggeredObjects.Remove(colliderObject);
            }
            else if (colliderObject is MovementAffectColliderObject)
            {
                ((MovementAffectColliderObject)colliderObject).ApplyMovement(entity.getEntityGameObject());
            }
        }
    }
}
